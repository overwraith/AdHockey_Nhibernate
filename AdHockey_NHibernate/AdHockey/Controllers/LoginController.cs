/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using AdHockey.Models;

namespace AdHockey.Controllers {
    public class LoginController : Controller {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingLoginAppender");

        // GET: Login
        public ActionResult Index() {
            Login login = new Models.Login();

            if (Request.Cookies.Get("AdHockeyLogin") != null) {
                login.EmailAddress = Request.Cookies.Get("AdHockeyLogin").Values["UserName"].ToString();
                //login.Password = Request.Cookies.Get("AdHockeyLogin").Values["Password"].ToString();
                login.RememberMe = bool.Parse(Request.Cookies.Get("AdHockeyLogin").Values["RememberMe"].ToString());
            }

            return View(login);
        }//end method

        [HttpGet]
        public ActionResult Login() {
            Login login = new Models.Login();

            if (Request.Cookies.Get("AdHockeyLogin") != null) {
                login.EmailAddress = Request.Cookies.Get("AdHockeyLogin").Values["UserName"].ToString();
                //login.Password = Request.Cookies.Get("AdHockeyLogin").Values["Password"].ToString();
                login.RememberMe = bool.Parse(Request.Cookies.Get("AdHockeyLogin").Values["RememberMe"].ToString());
            }

            return View(login);
        }//end method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Models.Login login) {

            if (login.RememberMe) {
                HttpCookie cookie = new HttpCookie("AdHockeyLogin");
                cookie.Values.Add("UserName", login.EmailAddress);
                cookie.Values.Add("RememberMe", login.RememberMe.ToString());
                cookie.Expires = DateTime.Now.AddDays(15);
                Response.Cookies.Add(cookie);
            }

            if (ModelState.IsValid) {
                if (login.IsValid()) {
                    //log the user login attempt
                    log.Debug(String.Format("User {0} logged in. ", login.EmailAddress));

                    Session["USER_NAME"] = login.EmailAddress.ToString();
                    Session["PASSWORD"] = login.Password.ToString();

                    return RedirectToAction("Index", "Home");
                }
                else {
                    ModelState.AddModelError("", "Incorrect email or password. ");
                }
            }
            return View("index", login);
        }//end method

    }//end class

}//end namespace