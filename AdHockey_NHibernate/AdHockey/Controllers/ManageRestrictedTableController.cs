using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageRestrictedTableController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public RestrictedTable ctRestrictedTable {
            get { return (RestrictedTable)System.Web.HttpContext.Current.Session["CT_RESTRICTED_TABLE"]; }
            set { System.Web.HttpContext.Current.Session["CT_RESTRICTED_TABLE"] = value; }
        }//end property

        public int GroupId {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_ID"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_ID"] = value; }
        }//end property

        // GET: ManageRestrictedTable
        public ActionResult Index(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedTableSearch = false;
            RestrictedTablePageNumber = 1;
            RestrictedTablePageSize = 2;

            GroupId = groupId;

            ctRestrictedTablePage = GetCurrentRestrictedTablePage();
            ViewBag.RestrictedTablePage = ctRestrictedTablePage;

            return View(ctRestrictedTable);
            //return View();
        }//end method

        public List<RestrictedTable> GetCurrentRestrictedTablePage() {
            List<RestrictedTable> list = null;
            using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetUsersPaged(RestrictedTablePageNumber, RestrictedTablePageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of RestrictedTable's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitRestrictedTable(RestrictedTable authTab) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
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

                ctRestrictedTablePage = GetCurrentRestrictedTablePage();
                ViewBag.RestrictedTablePage = ctRestrictedTablePage;

                ctRestrictedTable = null;

                return View("Index", ctRestrictedTable);
            }
            else {
                ctRestrictedTablePage = GetCurrentRestrictedTablePage();
                ViewBag.RestrictedTablePage = ctRestrictedTablePage;

                ctRestrictedTable = authTab;
                return View("Index", ctRestrictedTable);
            }

        }//end method

        public ActionResult DeleteRestrictedTable(int authTabId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedTable authTab = null;

            //delete RestrictedTable
            using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
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

            ctRestrictedTable = authTab;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedTableSetDataSource();
            ViewBag.RestrictedTablePage = ctRestrictedTablePage;
            ViewBag.RestrictedTableSearchStr = RestrictedTableSearchStr;

            return View("Index", ctRestrictedTable);
        }//end method
        #endregion

        #region RestrictedTableRelatedPaging
        public List<AdHockey.Models.RestrictedTable> ctRestrictedTablePage {
            get { return (List<RestrictedTable>)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsRestrictedTableSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"] = value; }
        }//end method

        public String RestrictedTableSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int RestrictedTablePageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int RestrictedTablePageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<RestrictedTable> GetRestrictedTableUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
                    return repo.GetUsersPaged(RestrictedTablePageNumber, RestrictedTablePageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public List<RestrictedTable> GetRestrictedTableSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchUsersPaged(RestrictedTableSearchStr, RestrictedTablePageNumber, RestrictedTablePageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedTableUnserarchedCount() {
            try {
                using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedTableSearchCount(String searchStr) {
            try {
                using (RestrictedTableRepository repo = new RestrictedTableRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(RestrictedTableSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void RestrictedTableSetDataSource() {
            try {
                if (IsRestrictedTableSearch) {
                    ctRestrictedTablePage = GetRestrictedTableSearchedPage(RestrictedTableSearchStr, RestrictedTablePageNumber, RestrictedTablePageSize);
                }
                else {
                    ctRestrictedTablePage = GetRestrictedTableUnsearchedPage(RestrictedTablePageNumber, RestrictedTablePageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctRestrictedTablePage = new List<RestrictedTable>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public ActionResult RestrictedTableFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedTablePageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTablePreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RestrictedTablePageNumber != 1) {
                --RestrictedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTableNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedTableItems = -1;

            if (IsRestrictedTableSearch)
                numRestrictedTableItems = GetRestrictedTableSearchCount(RestrictedTableSearchStr);
            else
                numRestrictedTableItems = GetRestrictedTableUnserarchedCount();

            if (Math.Ceiling((decimal)numRestrictedTableItems / (decimal)RestrictedTablePageSize) >= RestrictedTablePageNumber + 1) {
                ++RestrictedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTableLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedTables = -1;

            if (IsRestrictedTableSearch)
                numRestrictedTables = GetRestrictedTableSearchCount(RestrictedTableSearchStr);
            else
                numRestrictedTables = GetRestrictedTableUnserarchedCount();

            RestrictedTablePageNumber = (numRestrictedTables / RestrictedTablePageSize) + ((numRestrictedTables / RestrictedTablePageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedTableSearch = true;
            RestrictedTableSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedTableSearch = false;
            RestrictedTableSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace