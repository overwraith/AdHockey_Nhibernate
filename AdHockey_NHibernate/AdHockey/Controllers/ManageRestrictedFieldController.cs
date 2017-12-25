using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageRestrictedFieldController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public RestrictedField ctRestrictedField {
            get { return (RestrictedField)System.Web.HttpContext.Current.Session["CT_RESTRICTED_FIELD"]; }
            set { System.Web.HttpContext.Current.Session["CT_RESTRICTED_FIELD"] = value; }
        }//end property

        public int GroupId {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_ID"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_ID"] = value; }
        }//end property

        // GET: ManageRestrictedField
        public ActionResult Index(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedFieldSearch = false;
            RestrictedFieldPageNumber = 1;
            RestrictedFieldPageSize = 2;

            GroupId = groupId;

            ctRestrictedFieldPage = GetCurrentRestrictedFieldPage();
            ViewBag.RestrictedFieldPage = ctRestrictedFieldPage;

            return View(ctRestrictedField);
            //return View();
        }//end method

        public List<RestrictedField> GetCurrentRestrictedFieldPage() {
            List<RestrictedField> list = null;
            using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetRestrictedFieldsPaged(RestrictedFieldPageNumber, RestrictedFieldPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of RestrictedField's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitRestrictedField(RestrictedField authTab) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
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

                ctRestrictedFieldPage = GetCurrentRestrictedFieldPage();
                ViewBag.RestrictedFieldPage = ctRestrictedFieldPage;

                ctRestrictedField = null;

                return View("Index", ctRestrictedField);
            }
            else {
                ctRestrictedFieldPage = GetCurrentRestrictedFieldPage();
                ViewBag.RestrictedFieldPage = ctRestrictedFieldPage;

                ctRestrictedField = authTab;
                return View("Index", ctRestrictedField);
            }

        }//end method

        public ActionResult DeleteRestrictedField(int authTabId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedField authTab = null;

            //delete RestrictedField
            using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
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

            ctRestrictedField = authTab;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedFieldSetDataSource();
            ViewBag.RestrictedFieldPage = ctRestrictedFieldPage;
            ViewBag.RestrictedFieldSearchStr = RestrictedFieldSearchStr;

            return View("Index", ctRestrictedField);
        }//end method
        #endregion

        #region RestrictedFieldRelatedPaging
        public List<AdHockey.Models.RestrictedField> ctRestrictedFieldPage {
            get { return (List<RestrictedField>)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsRestrictedFieldSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"] = value; }
        }//end method

        public String RestrictedFieldSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int RestrictedFieldPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int RestrictedFieldPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<RestrictedField> GetRestrictedFieldUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
                    return repo.GetRestrictedFieldsPaged(RestrictedFieldPageNumber, RestrictedFieldPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public List<RestrictedField> GetRestrictedFieldSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchRestrictedFieldsPaged(RestrictedFieldSearchStr, RestrictedFieldPageNumber, RestrictedFieldPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedFieldUnserarchedCount() {
            try {
                using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumRestrictedFields();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedFieldSearchCount(String searchStr) {
            try {
                using (RestrictedFieldRepository repo = new RestrictedFieldRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(RestrictedFieldSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void RestrictedFieldSetDataSource() {
            try {
                if (IsRestrictedFieldSearch) {
                    ctRestrictedFieldPage = GetRestrictedFieldSearchedPage(RestrictedFieldSearchStr, RestrictedFieldPageNumber, RestrictedFieldPageSize);
                }
                else {
                    ctRestrictedFieldPage = GetRestrictedFieldUnsearchedPage(RestrictedFieldPageNumber, RestrictedFieldPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctRestrictedFieldPage = new List<RestrictedField>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public ActionResult RestrictedFieldFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedFieldPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RestrictedFieldPageNumber != 1) {
                --RestrictedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedFieldItems = -1;

            if (IsRestrictedFieldSearch)
                numRestrictedFieldItems = GetRestrictedFieldSearchCount(RestrictedFieldSearchStr);
            else
                numRestrictedFieldItems = GetRestrictedFieldUnserarchedCount();

            if (Math.Ceiling((decimal)numRestrictedFieldItems / (decimal)RestrictedFieldPageSize) >= RestrictedFieldPageNumber + 1) {
                ++RestrictedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedFields = -1;

            if (IsRestrictedFieldSearch)
                numRestrictedFields = GetRestrictedFieldSearchCount(RestrictedFieldSearchStr);
            else
                numRestrictedFields = GetRestrictedFieldUnserarchedCount();

            RestrictedFieldPageNumber = (numRestrictedFields / RestrictedFieldPageSize) + ((numRestrictedFields / RestrictedFieldPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedFieldSearch = true;
            RestrictedFieldSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedFieldSearch = false;
            RestrictedFieldSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace