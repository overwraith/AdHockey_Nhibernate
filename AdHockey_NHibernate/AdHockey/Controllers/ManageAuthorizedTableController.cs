using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageAuthorizedTableController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public AuthorizedTable ctAuthorizedTable {
            get { return (AuthorizedTable)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE"] = value; }
        }//end property

        public int GroupId {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_ID"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_ID"] = value; }
        }//end property

        // GET: ManageAuthorizedTable
        public ActionResult Index(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedTableSearch = false;
            AuthorizedTablePageNumber = 1;
            AuthorizedTablePageSize = 2;

            GroupId = groupId;

            ctAuthorizedTablePage = GetCurrentAuthorizedTablePage();
            ViewBag.AuthorizedTablePage = ctAuthorizedTablePage;

            return View(ctAuthorizedTable);
            //return View();
        }//end method

        public List<AuthorizedTable> GetCurrentAuthorizedTablePage() {
            List<AuthorizedTable> list = null;
            using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetTablesPaged(AuthorizedTablePageNumber, AuthorizedTablePageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of AuthorizedTable's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitAuthorizedTable(AuthorizedTable authTab) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
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

                ctAuthorizedTablePage = GetCurrentAuthorizedTablePage();
                ViewBag.AuthorizedTablePage = ctAuthorizedTablePage;

                ctAuthorizedTable = null;

                return View("Index", ctAuthorizedTable);
            }
            else {
                ctAuthorizedTablePage = GetCurrentAuthorizedTablePage();
                ViewBag.AuthorizedTablePage = ctAuthorizedTablePage;

                ctAuthorizedTable = authTab;
                return View("Index", ctAuthorizedTable);
            }

        }//end method

        public ActionResult DeleteAuthorizedTable(int authTabId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedTable authTab = null;

            //delete AuthorizedTable
            using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
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

            ctAuthorizedTable = authTab;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedTableSetDataSource();
            ViewBag.AuthorizedTablePage = ctAuthorizedTablePage;
            ViewBag.AuthorizedTableSearchStr = AuthorizedTableSearchStr;

            return View("Index", ctAuthorizedTable);
        }//end method
        #endregion

        #region AuthorizedTableRelatedPaging
        public List<AdHockey.Models.AuthorizedTable> ctAuthorizedTablePage {
            get { return (List<AuthorizedTable>)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsAuthorizedTableSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"] = value; }
        }//end method

        public String AuthorizedTableSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int AuthorizedTablePageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int AuthorizedTablePageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<AuthorizedTable> GetAuthorizedTableUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
                    return repo.GetTablesPaged(AuthorizedTablePageNumber, AuthorizedTablePageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public List<AuthorizedTable> GetAuthorizedTableSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchTablesPaged(AuthorizedTableSearchStr, AuthorizedTablePageNumber, AuthorizedTablePageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedTableUnserarchedCount() {
            try {
                using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedTableSearchCount(String searchStr) {
            try {
                using (AuthorizedTableRepository repo = new AuthorizedTableRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(AuthorizedTableSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void AuthorizedTableSetDataSource() {
            try {
                if (IsAuthorizedTableSearch) {
                    ctAuthorizedTablePage = GetAuthorizedTableSearchedPage(AuthorizedTableSearchStr, AuthorizedTablePageNumber, AuthorizedTablePageSize);
                }
                else {
                    ctAuthorizedTablePage = GetAuthorizedTableUnsearchedPage(AuthorizedTablePageNumber, AuthorizedTablePageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctAuthorizedTablePage = new List<AuthorizedTable>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public ActionResult AuthorizedTableFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedTablePageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedTablePreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (AuthorizedTablePageNumber != 1) {
                --AuthorizedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedTableNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedTableItems = -1;

            if (IsAuthorizedTableSearch)
                numAuthorizedTableItems = GetAuthorizedTableSearchCount(AuthorizedTableSearchStr);
            else
                numAuthorizedTableItems = GetAuthorizedTableUnserarchedCount();

            if (Math.Ceiling((decimal)numAuthorizedTableItems / (decimal)AuthorizedTablePageSize) >= AuthorizedTablePageNumber + 1) {
                ++AuthorizedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedTableLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedTables = -1;

            if (IsAuthorizedTableSearch)
                numAuthorizedTables = GetAuthorizedTableSearchCount(AuthorizedTableSearchStr);
            else
                numAuthorizedTables = GetAuthorizedTableUnserarchedCount();

            AuthorizedTablePageNumber = (numAuthorizedTables / AuthorizedTablePageSize) + ((numAuthorizedTables / AuthorizedTablePageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedTableSearch = true;
            AuthorizedTableSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedTableSearch = false;
            AuthorizedTableSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace