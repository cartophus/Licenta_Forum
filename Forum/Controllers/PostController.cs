using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    
    public class PostController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        /*public ActionResult Show(int id)
        {
            Post post = db.Posts.Find(id);
            Thread thread = db.Threads.Find(post.ThreadId);
            ViewBag.Post = post;
            ViewBag.Thread = thread; // la ce thread e atasat post-ul 

            ViewBag.afisareButoane = false;
            if (User.IsInRole("Editor") || User.IsInRole("Administrator"))
            {
                ViewBag.afisareButoane = true;
            }
            ViewBag.esteAdmin = User.IsInRole("Administrator");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();

            return View(post);
        }*/

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(int id)
        {
            Post post = new Post();
            post.Thread = db.Threads.Find(id);
            //TO DO verifica daca exista threadul inainte sa mergi mai departe
            post.UserId = User.Identity.GetUserId();
            post.ThreadId = post.Thread.ThreadId;
            return View(post);
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        [HttpPost]
        public ActionResult New(Post post)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    db.Posts.Add(post);
                    db.SaveChanges();
                    TempData["message"] = "Post added!";
                    return RedirectToAction("Show","Thread",new { @id = post.ThreadId });

                }
                else
                {
                    return View(post);
                }
            }
            catch (Exception e)
            {
                return View(post);
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Edit(int id)
        {

            Post post = db.Posts.Find(id);
            ViewBag.Post = post;

            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "You have no rights to modify!";
                return RedirectToAction("Show","Thread", new { @id = post.ThreadId});
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Post requestPost)
        {
            Post post = db.Posts.Find(id);
            try
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(post))
                    {
                        post.Title = requestPost.Title;
                        post.Content = requestPost.Content;
                        db.SaveChanges();
                        TempData["message"] = "Post modified!";
                    }
                    return RedirectToAction("Show", "Thread", new { @id = post.ThreadId });
                }
                else
                {
                    return View(post);
                }

            }
            catch (Exception e)
            {
                return View(post);
            }
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {

            Post post = db.Posts.Find(id);

            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Posts.Remove(post);
                db.SaveChanges();
                return RedirectToAction("Show", "Thread", new { @id = post.ThreadId });
            }
            else
            {
                TempData["message"] = "You don't have the right to delete!";
                return RedirectToAction("Show", "Thread", new { @id = post.ThreadId });
            }
        }
    }
}