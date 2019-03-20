using Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class VoteThreadController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        public ActionResult ShowThumbsUp (int id)
        {
            int countThumbsUp = db.VoteThreads.Where(vt => vt.Thread.ThreadId == id && vt.Opinion==1).Count();

            ViewBag.CountThumbsUp = countThumbsUp;

            return PartialView("_ShowThumbsUp");
        }

        public ActionResult ShowThumbsDown(int id)
        {
            int countThumbsDown = db.VoteThreads.Where(vt => vt.Thread.ThreadId == id && vt.Opinion==0).Count();

            ViewBag.CountThumbsDown = countThumbsDown;

            return PartialView("_ShowThumbsDown");
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(int id,int opinion)
        {
            VoteThread vote = new VoteThread();
            
            string userId = User.Identity.GetUserId();

            var entry = db.VoteThreads.Where(vt => vt.User.Id == userId);
            bool found = entry.Any();

            if(found)
            {
                foreach(VoteThread v in entry)
                    db.VoteThreads.Remove(v);

                db.SaveChanges();
            }

            vote.User = db.Users.Find(userId);

            vote.Thread = db.Threads.Find(id);
            vote.Opinion = opinion;

            db.VoteThreads.Add(vote);
            db.SaveChanges();


            int countThumbsDown = db.VoteThreads.Where(vt => vt.Thread.ThreadId == id && vt.Opinion==0).Count();
            ViewBag.CountThumbsDown = countThumbsDown;

            int countThumbsUp = db.VoteThreads.Where(vt => vt.Thread.ThreadId == id && vt.Opinion==1).Count();
            ViewBag.CountThumbsUp = countThumbsUp;

            return RedirectToAction("Show", "Thread", new { id });
        }
    }
}