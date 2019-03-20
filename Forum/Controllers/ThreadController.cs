using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.IO;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using ASPNET_MVC_ChartsDemo.Models;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using Microsoft.ML.Trainers;
using Microsoft.ML.Model;

namespace Forum.Controllers
{
    public class ThreadController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        private static readonly string _trainDataPath = "C:\\Users\\vladi\\Desktop\\work\\An3\\Sem1\\DAW\\Forum de Discutii\\Forum\\Forum\\DataAi\\votesTrain.csv";
        private static readonly string _testDataPath = "C:\\Users\\vladi\\Desktop\\work\\An3\\Sem1\\DAW\\Forum de Discutii\\Forum\\Forum\\DataAi\\votesTest.csv";
        private static readonly string _modelPath = "C:\\Users\\vladi\\Desktop\\work\\An3\\Sem1\\DAW\\Forum de Discutii\\Forum\\Forum\\DataAi\\Model.zip";

        [NonAction]
        public static ITransformer Train(MLContext context)
        {
            IDataView dataView = context.Data.LoadFromTextFile<VoteThreadMask>(_trainDataPath, hasHeader: true, separatorChar: ',');

            var pipeline = context.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Opinion")
                .Append(context.Transforms.Categorical.OneHotEncoding(outputColumnName: "VoteThreadIdEncoded", inputColumnName: "VoteThreadId"))
                .Append(context.Transforms.Categorical.OneHotEncoding(outputColumnName: "UserIdEncoded", inputColumnName: "UserId"))
                .Append(context.Transforms.Categorical.OneHotEncoding(outputColumnName: "ThreadIdEncoded", inputColumnName: "ThreadId"))
                .Append(context.Transforms.Concatenate("Features", "VoteThreadId", "UserIdEncoded", "ThreadIdEncoded"))
                .Append(context.Regression.Trainers.PoissonRegression());

            var model = pipeline.Fit(dataView);
            SaveModelAsFile(context, model);
            return model;
                
        }

        [NonAction]
        private static void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            using (var fileStream = new FileStream(_modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fileStream);
            Console.WriteLine("The model is saved to {0}", _modelPath);
        }

        [NonAction]
        public static void Evaluate(MLContext context, ITransformer model)
        {
            IDataView dataView = context.Data.LoadFromTextFile<VoteThreadMask>(_testDataPath, hasHeader: true, separatorChar: ',');

            var predictions = model.Transform(dataView);

            var metrics = context.Regression.Evaluate(predictions, "Label", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");

            Console.WriteLine($"*       R2 Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       RMS loss:      {metrics.Rms:#.##}");
        }

        [NonAction]
        private static VoteThreadPrediction TestSinglePrediction(MLContext mlContext, VoteThread voteThread)
        {
            VoteThreadMask vtMask = new VoteThreadMask();
            vtMask.VoteThreadId = voteThread.VoteThreadId;
            vtMask.UserId = voteThread.User.Id;
            vtMask.ThreadId = voteThread.Thread.ThreadId;
            vtMask.Opinion = voteThread.Opinion;


            ITransformer loadedModel;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }
            var predictionFunction = loadedModel.CreatePredictionEngine<VoteThreadMask, VoteThreadPrediction>(mlContext);

            var prediction = predictionFunction.Predict(vtMask);

            Console.WriteLine($"**********************************************************************");
            Console.WriteLine($"Predicted fare: {prediction.OpinionPrediction:0.####}, actual fare: 15.5");
            Console.WriteLine($"**********************************************************************");

            return prediction;
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Recommendations()
        { 
            var context = new MLContext(seed: 0);

            var model = Train(context);

            Evaluate(context, model);
            // ---------------------------------------
            var votes = (from vote in db.VoteThreads where vote.User.Id == User.Identity.GetUserId() select vote).ToList();

            List<VoteThreadPrediction> votePredictions = null;

            foreach(VoteThread vote in votes)
            {
                votePredictions.Add(TestSinglePrediction(context, vote));
            }

            votePredictions = votePredictions.OrderByDescending(o => o.OpinionPrediction).ToList();

            //List<Thread> threads = null;

            //for(int i = 0 ; i < 5; i++)
            //{
            //    VoteThreadPrediction vtp = votePredictions[i];
            //    VoteThread vt = db.VoteThreads.Find(vtp.label);
            //    threads.Add(db.Threads.Find(vt.Thread.ThreadId));
            //}

            //ViewBag.Threads = threads;

            return View();
        }

        public ActionResult Index(int? page)
        {
            var threads = from thread in db.Threads.Include("Category").Include("User")
                          orderby thread.Created descending
                          select thread;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            ViewBag.Function = "Index";

            // **Data needed for the pie chart**
            List<DataPoint> dataPoints = new List<DataPoint>();

            var categs = from category in db.Categories
                         select category;
            foreach(Category cat in categs)
            {
                string dataName = cat.CategoryName;
                double dataCount = 0;
                foreach (Thread thread in threads)
                    if (thread.CategoryId == cat.CategoryId)
                        dataCount++;
                dataPoints.Add(new DataPoint(dataName, dataCount));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            //**END**

            return View(threads.ToPagedList(page ?? 1, 7));
        }

        public ActionResult IndexByCategory(int id, int? page)
        {
            var threads = from thread in db.Threads.Include("Category").Include("User")
                          where thread.CategoryId == id
                          orderby thread.Created descending
                          select thread;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            if (!threads.Any()) ViewBag.Validator = false;
            else ViewBag.Validator = true;

            ViewBag.Category = db.Categories.Find(id);

            return View(threads.ToPagedList(page ?? 1, 7));
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int id)
        {
            Thread thread = db.Threads.Find(id);
            ViewBag.Thread = thread;
            ViewBag.Category = thread.Category;

            var posts = from post in db.Posts
                        where post.ThreadId == thread.ThreadId
                        select post;
            ViewBag.Posts = posts;
            ViewBag.Validator = posts.Any();

            ViewBag.afisareButoane = false;
            if (User.IsInRole("Editor") || User.IsInRole("Administrator"))
            {
                ViewBag.afisareButoane = true;
            }

            ViewBag.esteAdmin = User.IsInRole("Administrator");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.Function = "Show";
            return View(thread);

        }

        public JsonResult AutocompleteSuggestions(string term)
        {
            var threads = db.Threads.Where(x => x.Title.Contains(term)).Select(x => x.Title).ToList();

            return Json(threads, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Search(string searchString)
        {
            var threads = from thread in db.Threads
                          select thread;

            ViewBag.SearchString = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                threads = threads.Where(t => t.Title.Contains(searchString) || t.Category.CategoryName.Contains(searchString));
                ViewBag.Threads = threads;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(int? id)
        {
            Thread thread = new Thread();
            thread.Categories = GetAllCategories();
            thread.UserId = User.Identity.GetUserId();

            if (id.HasValue)
            {
                thread.CategoryId = (int)id;
                ViewBag.Validator = true;
                ViewBag.CategoryId = id;
            }
            else ViewBag.Validator = false;
            return View(thread);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();
            // Extragem toate categoriile din baza de date
            var categories = from cat in db.Categories select cat;
            // iteram prin categorii
            foreach (var category in categories)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            // returnam lista de categorii
            return selectList;
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        [HttpPost]
        public ActionResult New(Thread thread, int? id)
        {
            thread.Categories = GetAllCategories();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Threads.Add(thread);
                    db.SaveChanges();
                    TempData["message"] = "Thread added!";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (id.HasValue)
                    {
                        thread.CategoryId = (int)id;
                        ViewBag.Validator = true;
                        ViewBag.CategoryId = id;
                    }
                    else ViewBag.Validator = false;
                    return View(thread);
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                if (id.HasValue)
                {
                    thread.CategoryId = (int)id;
                    ViewBag.Validator = true;
                    ViewBag.CategoryId = id;
                }
                else ViewBag.Validator = false;
                return View(thread);
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Edit(int id)
        {

            Thread thread = db.Threads.Find(id);
            thread.Categories = GetAllCategories();
            ViewBag.Thread = thread;

            if (thread.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(thread);
            }
            else
            {
                TempData["message"] = "You have no rights to modify!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Thread requestThread)
        {
            Thread thread = db.Threads.Find(id);
            try
            {
                if (ModelState.IsValid)
                {

                    if (TryUpdateModel(thread))
                    {
                        thread.Title = requestThread.Title;
                        thread.Content = requestThread.Content;
                        thread.CategoryId = requestThread.CategoryId;
                        db.SaveChanges();
                        TempData["message"] = "Thread modified!";
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return View(thread);
                }

            }
            catch (Exception e)
            {
                return View(thread);
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {

            Thread thread = db.Threads.Find(id);


            if (thread.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Threads.Remove(thread);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You don't have the right to delete!";
                return RedirectToAction("Index");
            }
        }
    }
}