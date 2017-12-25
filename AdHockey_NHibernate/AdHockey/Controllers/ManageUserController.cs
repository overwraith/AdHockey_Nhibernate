using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageUserController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public User ctUser {
            get { return (User)System.Web.HttpContext.Current.Session["CT_USER"]; }
            set { System.Web.HttpContext.Current.Session["CT_USER"] = value; }
        }//end property

        // GET: ManageUser
        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUserSearch = false;
            UserPageNumber = 1;
            UserPageSize = 2;

            ctUserPage = GetCurrentUserPage();
            ViewBag.UserPage = ctUserPage;

            return View(ctUser);
            //return View();
        }//end method

        public List<User> GetCurrentUserPage() {
            List<User> list = null;
            using (UserRepository repo = new UserRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetUsersPaged(UserPageNumber, UserPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of users. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitUser(User user) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (UserRepository repo = new UserRepository()) {
                        repo.BeginTransaction();
                        repo.Insert(user);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error inserting user into database. ", ex);
                    throw;
                }
                ModelState.Clear();

                return View("Index", ctUser);
            }
            else {
                ctUserPage = GetCurrentUserPage();
                ViewBag.UserPage = ctUserPage;

                ctUser = user;
                return View("Index", ctUser);
            }

        }//end method

        public ActionResult DeleteUser(int UserId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            User user = null;
            //delete User
            using (UserRepository repo = new UserRepository()) {
                try {
                    repo.BeginTransaction();
                    user = repo.GetById(UserId);
                    repo.Delete(UserId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting user from database. ", ex);
                    throw;
                }
            }

            ctUser = user;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            UserSetDataSource();
            ViewBag.UserPage = ctUserPage;
            ViewBag.UserSearchStr = UserSearchStr;

            return View("Index", ctUser);
        }//end method
        #endregion

        #region UserRelatedPaging
        public List<AdHockey.Models.User> ctUserPage {
            get { return (List<User>)System.Web.HttpContext.Current.Session["CT_USER_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_USER_PAGE"] = value; }
        }//end method

        public bool IsUserSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_USER_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_USER_SEARCH"] = value; }
        }//end method

        public String UserSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["USER_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["USER_SEARCH_STRING"] = value; }
        }//end method

        public int UserPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["USER_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["USER_PAGE_NUMBER"] = value; }
        }//end method

        public int UserPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["USER_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["USER_PAGE_SIZE"] = value; }
        }//end method

        public List<User> GetUserUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetUsersPaged(UserPageNumber, UserPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }
        }//end method

        public List<User> GetUserSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchUsersPaged(UserSearchStr, UserPageNumber, UserPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }
        }//end method

        public int GetUserUnserarchedCount() {
            try {
                using (UserRepository repo = new UserRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of users. ", ex);
                throw;
            }
        }//end method

        public int GetUserSearchCount(String searchStr) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(UserSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void UserSetDataSource() {
            try {
                if (IsUserSearch) {
                    ctUserPage = GetUserSearchedPage(UserSearchStr, UserPageNumber, UserPageSize);
                }
                else {
                    ctUserPage = GetUserUnsearchedPage(UserPageNumber, UserPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctUserPage = new List<User>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }
        }//end method

        public ActionResult UserFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            UserPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult UserPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (UserPageNumber != 1) {
                --UserPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult UserNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numTemplateItems = -1;

            if (IsUserSearch)
                numTemplateItems = GetUserSearchCount(UserSearchStr);
            else
                numTemplateItems = GetUserUnserarchedCount();

            if (Math.Ceiling((decimal)numTemplateItems / (decimal)UserPageSize) >= UserPageNumber + 1) {
                ++UserPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult UserLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numUsers = -1;

            if (IsUserSearch)
                numUsers = GetUserSearchCount(UserSearchStr);
            else
                numUsers = GetUserUnserarchedCount();

            UserPageNumber = (numUsers / UserPageSize) + ((numUsers / UserPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUserSearch = true;
            UserSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUserSearch = false;
            UserSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace