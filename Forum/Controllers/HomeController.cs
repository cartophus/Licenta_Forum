using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Forum.Models;
using System.Net;
using System.Net.Mail;
using System.Configuration;

namespace Forum.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(Forum.Models.Email model)
        {
            string mailAccount = ConfigurationManager.AppSettings["mailAccount"];
            string password = ConfigurationManager.AppSettings["mailPassword"];

            try
            {
                MailMessage mailMsg = new MailMessage(mailAccount, mailAccount);
                mailMsg.Subject = model.Subject;
                mailMsg.Body = model.Body;
                mailMsg.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;

                NetworkCredential nc = new NetworkCredential(mailAccount, password);
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = nc;
                smtp.Send(mailMsg);

                TempData["MailSent"] = "Your feedback has been sent!";

            }
            catch(Exception e)
            {
                TempData["MailFail"] = "There was an error in sending your feedback. Please try again later.";
            }
            return RedirectToAction("Contact", "Home");
        }
    }
}