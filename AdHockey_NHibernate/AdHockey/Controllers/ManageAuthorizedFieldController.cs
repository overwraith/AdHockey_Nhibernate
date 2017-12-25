using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageAuthorizedFieldController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public AuthorizedField ctAuthorizedField {
            get { return (AuthorizedField)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_FIELD"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_FIELD"] = value; }
        }//end property

        public int GroupId {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_ID"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_ID"] = value; }
        }//end property

        // GET: ManageAuthorizedField
        public ActionResult Index(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedFieldSearch = false;
            AuthorizedFieldPageNumber = 1;
            AuthorizedFieldPageSize = 2;

            GroupId = groupId;

            ctAuthorizedFieldPage = GetCurrentAuthorizedFieldPage();
            ViewBag.AuthorizedFieldPage = ctAuthorizedFieldPage;

            return View(ctAuthorizedField);
            //return View();
        }//end method

        public List<AuthorizedField> GetCurrentAuthorizedFieldPage() {
            List<AuthorizedField> list = null;
            using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetUsersPaged(AuthorizedFieldPageNumber, AuthorizedFieldPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of AuthorizedField's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitAuthorizedField(AuthorizedField authTab) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                        repo.BeginTransaction();

                        Group group = null;
                        using (GroupRepository grpRepo = new GroupRepository()) {
                            grpRepo.BeginTransaction();
                            group = grpRepo.GetById(GroupId);
                        }

                        authTab.GroupId = GroupId;
                        authTab.Group = group;
                        repo.Insert(authTab);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error Submitting authorized table. ", ex);
                    throw;
                }
                ModelState.Clear();

                ctAuthorizedFieldPage = GetCurrentAuthorizedFieldPage();
                ViewBag.AuthorizedFieldPage = ctAuthorizedFieldPage;

                ctAuthorizedField = null;

                return View("Index", ctAuthorizedField);
            }
            else {
                ctAuthorizedFieldPage = GetCurrentAuthorizedFieldPage();
                ViewBag.AuthorizedFieldPage = ctAuthorizedFieldPage;

                ctAuthorizedField = authTab;
                return View("Index", ctAuthorizedField);
            }

        }//end method

        public ActionResult DeleteAuthorizedField(int authTabId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedField authTab = null;

            //delete AuthorizedField
            using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                try {
                    repo.BeginTransaction();
                    authTab = repo.GetById(authTabId);
                    repo.Delete(authTabId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting authorized table. ", ex);
                    throw;
                }
            }

            ctAuthorizedField = authTab;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedFieldSetDataSource();
            ViewBag.AuthorizedFieldPage = ctAuthorizedFieldPage;
            ViewBag.AuthorizedFieldSearchStr = AuthorizedFieldSearchStr;

            return View("Index", ctAuthorizedField);
        }//end method
        #endregion

        #region AuthorizedFieldRelatedPaging
        public List<AdHockey.Models.AuthorizedField> ctAuthorizedFieldPage {
            get { return (List<AuthorizedField>)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsAuthorizedFieldSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"] = value; }
        }//end method

        public String AuthorizedFieldSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int AuthorizedFieldPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int AuthorizedFieldPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<AuthorizedField> GetAuthorizedFieldUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                    return repo.GetUsersPaged(AuthorizedFieldPageNumber, AuthorizedFieldPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public List<AuthorizedField> GetAuthorizedFieldSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchFieldsPaged(AuthorizedFieldSearchStr, AuthorizedFieldPageNumber, AuthorizedFieldPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedFieldUnserarchedCount() {
            try {
                using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedFieldSearchCount(String searchStr) {
            try {
                using (AuthorizedFieldRepository repo = new AuthorizedFieldRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(AuthorizedFieldSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void AuthorizedFieldSetDataSource() {
            try {
                if (IsAuthorizedFieldSearch) {
                    ctAuthorizedFieldPage = GetAuthorizedFieldSearchedPage(AuthorizedFieldSearchStr, AuthorizedFieldPageNumber, AuthorizedFieldPageSize);
                }
                else {
                    ctAuthorizedFieldPage = GetAuthorizedFieldUnsearchedPage(AuthorizedFieldPageNumber, AuthorizedFieldPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctAuthorizedFieldPage = new List<AuthorizedField>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public ActionResult AuthorizedFieldFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedFieldPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedFieldPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (AuthorizedFieldPageNumber != 1) {
                --AuthorizedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedFieldNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedFieldItems = -1;

            if (IsAuthorizedFieldSearch)
                numAuthorizedFieldItems = GetAuthorizedFieldSearchCount(AuthorizedFieldSearchStr);
            else
                numAuthorizedFieldItems = GetAuthorizedFieldUnserarchedCount();

            if (Math.Ceiling((decimal)numAuthorizedFieldItems / (decimal)AuthorizedFieldPageSize) >= AuthorizedFieldPageNumber + 1) {
                ++AuthorizedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedFieldLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedFields = -1;

            if (IsAuthorizedFieldSearch)
                numAuthorizedFields = GetAuthorizedFieldSearchCount(AuthorizedFieldSearchStr);
            else
                numAuthorizedFields = GetAuthorizedFieldUnserarchedCount();

            AuthorizedFieldPageNumber = (numAuthorizedFields / AuthorizedFieldPageSize) + ((numAuthorizedFields / AuthorizedFieldPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedFieldSearch = true;
            AuthorizedFieldSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedFieldSearch = false;
            AuthorizedFieldSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace