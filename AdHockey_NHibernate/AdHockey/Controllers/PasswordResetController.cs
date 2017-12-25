/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey;
using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class PasswordResetController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");
        private static readonly log4net.ILog useLog = log4net.LogManager.GetLogger("RollingUsageAppender");

        [AllowAnonymous]
        public ActionResult Index() {
            return View(new ForegotPasswordViewModel());
        }//end method

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForegotPassword(ForegotPasswordViewModel model) {

            if (ModelState.IsValid) {
                useLog.Debug(String.Format("User password reset attempt, email address: {0}", model.EmailAddress));

                //get user object via email address. 
                User user = null;
                try {
                    using (UserRepository usrRepo = new UserRepository()) {
                        usrRepo.BeginTransaction();
                        user = usrRepo.GetByEmail(model.EmailAddress);
                    }
                }
                catch (Exception ex) {
                    log.Debug(String.Format("Error resetting password, email address: {0}", model.EmailAddress), ex);
                    throw;
                }

                //don't reveal that the user does not exist or is not confirmed
                if (user == null)
                    return View("ForegotPasswordConfirmation");

                //generate a new password
                String pass = System.Web.Security.Membership.GeneratePassword(8, 3);

                //generate callback url
                String callbackUrl = Url.Action("ResetPassword", "PasswordReset", new { userId = user.UserId, pass = pass});

                //send email
                MailSender sender = new MailSender();
                sender.Send(user.EmailAddress, 
                    "Reset Password", 
                    String.Format("Please reset your password by clicking <a href=\"{0}\">here</a>", callbackUrl));

                try { 
                    //set user password to the newly generated password
                    user.SetPassword(pass);
                    using (UserRepository usrRepo = new UserRepository()) {
                        usrRepo.BeginTransaction();
                        usrRepo.Insert(user);
                    }
                }
                catch (Exception ex) {
                    log.Debug(String.Format("Error during password reset insertion of temp password email: {0}", user.EmailAddress), ex);
                    throw;
                }
            }
            else {
                return View("Index", model);
            }

            return View("index", model);
        }//end method

        [AllowAnonymous]
        public ActionResult ResetPassword(int userId, String pass) {

            //get user object via email address. 
            User user = null;
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();
                user = usrRepo.GetById(userId);
            }

            if (user.ComparePassword(pass)) {
                useLog.Debug(String.Format("User temporary password assigned, email address: {0}", user.EmailAddress));
                return View("ForegotPasswordConfirmation", new ForegotPasswordViewModel() { EmailAddress = user.EmailAddress });
            }

            return View("index", "PasswordReset");
        }//end method

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPassword(ForegotPasswordViewModel model) {
            //view model self validates the passwors are the same
            if (ModelState.IsValid) {
                //get user object via email address. 
                User user = null;
                using (UserRepository usrRepo = new UserRepository()) {
                    usrRepo.BeginTransaction();
                    user = usrRepo.GetByEmail(model.EmailAddress);
                }

                user.SetPassword(model.PasswordA);

                //re-insert user into database
                try { 
                    using (UserRepository usrRepo = new UserRepository()) {
                        usrRepo.BeginTransaction();
                        usrRepo.Insert(user);
                    }
                }
                catch (Exception ex) {
                    log.Debug(String.Format("Error during password reset insertion of user email: {0}", model.EmailAddress), ex);
                    throw;
                }

                //log user in and forward them to the home page
                Session["USER_NAME"] = user.EmailAddress;

                useLog.Debug(String.Format("User password reset concluded, email address: {0}", model.EmailAddress));

                return RedirectToAction("index", "Home");
            }
            else
                return View(model);
        }//end method

    }//end class

}//end namespace