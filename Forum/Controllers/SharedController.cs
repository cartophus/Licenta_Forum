using Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class SharedController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();
        
        public ActionResult Header()
        {
            var categories = from category in db.Categories
                             select category;
            ViewBag.Categories = categories;
            if (User.IsInRole("Administrator")) ViewBag.afisare = true;
            return PartialView("_Header");
        }
    }
}