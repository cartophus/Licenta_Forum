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

        [Authorize]
        public ActionResult NearbyThreads()
        {

            return View();
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

            //initialized in case of browser not supporting geolocation
            thread.Latitude = 0;
            thread.Longitude = 0;

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