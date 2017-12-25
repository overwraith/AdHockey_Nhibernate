using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers
{
    public class ManageBulkTemplateController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public BulkTemplate ctUser {
            get { return (BulkTemplate)System.Web.HttpContext.Current.Session["CT_User"]; }
            set { System.Web.HttpContext.Current.Session["CT_User"] = value; }
        }//end property

        public int ReportId {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_ID"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_ID"] = value; }
        }//end property

        // GET: ManageBulkTemplate
        public ActionResult Index(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ReportId = reportId;
            IsUserSearch = false;
            UserPageNumber = 1;
            UserPageSize = 2;

            ctUserPage = GetCurrentUserPage();
            ViewBag.UserPage = ctUserPage;

            return View(ctUser);
            //return View();
        }//end method

        public List<BulkTemplate> GetCurrentUserPage() {
            List<BulkTemplate> list = null;
            using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                try {
                    list = repo.GetUsersPaged(UserPageNumber, UserPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of Users. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitUser(BulkTemplate User) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                        repo.BeginTransaction();
                        Report report = null;
                        using (ReportRepository rptRepo = new ReportRepository()) {
                            rptRepo.BeginTransaction();
                            report = rptRepo.GetById(ReportId);
                        }

                        //assign the report object
                        User.Report = report;

                        //insert into database
                        repo.Insert(User);
                        repo.CommitTransaction();
                    }
                }
                catch (Exception ex) {
                    log.Error("Error in SubmitUser method. ", ex);
                    throw;
                }

                ModelState.Clear();

                ctUserPage = GetCurrentUserPage();
                ViewBag.UserPage = ctUserPage;

                return View("Index", ctUser);
            }
            else {
                //return malformed object to user
                ctUserPage = GetCurrentUserPage();
                ViewBag.UserPage = ctUserPage;

                ctUser = User;
                return View("Index", ctUser);
            }

        }//end method

        public ActionResult DeleteUser(int UserId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            BulkTemplate User = null;

            //delete User
            using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                try {
                    User = repo.GetById(UserId);
                    repo.Delete(UserId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting User. ", ex);
                    throw;
                }
            }

            ctUser = User;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            BulkTemplateSetDataSource(ReportId);
            ViewBag.UserPage = ctUserPage;
            ViewBag.UserSearchStr = UserSearchStr;

            return View("Index", ctUser);
        }//end method
        #endregion

        #region BulkTemplateRelatedPaging
        public List<AdHockey.Models.BulkTemplate> ctUserPage {
            get { return (List<BulkTemplate>)System.Web.HttpContext.Current.Session["CT_BULK_TEMPLATE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_BULK_TEMPLATE_PAGE"] = value; }
        }//end method

        public bool IsUserSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_BULK_TEMPLATE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_BULK_TEMPLATE_SEARCH"] = value; }
        }//end method

        public String UserSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["BULK_TEMPLATE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["BULK_TEMPLATE_SEARCH_STRING"] = value; }
        }//end method

        public int UserPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["BULK_TEMPLATE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["BULK_TEMPLATE_PAGE_NUMBER"] = value; }
        }//end method

        public int UserPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["BULK_TEMPLATE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["BULK_TEMPLATE_PAGE_SIZE"] = value; }
        }//end method

        public List<BulkTemplate> GetUserUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                    return repo.GetUsersPaged(UserPageNumber, UserPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting Users paged. ", ex);
                throw;
            }
        }//end method

        public List<BulkTemplate> GetUserSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                    return repo.SearchUsersPaged(UserSearchStr, UserPageNumber, UserPageSize, ReportId).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error searching for page of Users. ", ex);
                throw;
            }
        }//end method

        public int GetUserUnserarchedCount() {
            try {
                using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                    return repo.GetNumUsers();
                }
            }
            catch(Exception ex){
                log.Error("Error getting number of Users. ", ex);
                throw;
            }
        }//end method

        public int GetUserSearchCount(String searchStr) {
            try {
                using (BulkTemplateRepository repo = new BulkTemplateRepository()) {
                    return repo.GetTotalNumSearchResults(UserSearchStr, ReportId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void BulkTemplateSetDataSource(int reportId) {
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
                ctUserPage = new List<BulkTemplate>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of Users. ", ex);
                throw;
            }
        }//end method

        public ActionResult UserFirstPage() {
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

            IsUserSearch = true;
            UserSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
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