using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageRestrictedSchemaController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public RestrictedSchema ctRestrictedSchema {
            get { return (RestrictedSchema)System.Web.HttpContext.Current.Session["CT_RESTRICTED_SCHEMA"]; }
            set { System.Web.HttpContext.Current.Session["CT_RESTRICTED_SCHEMA"] = value; }
        }//end property

        public int GroupId {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_ID"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_ID"] = value; }
        }//end property

        // GET: ManageRestrictedSchema
        public ActionResult Index(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedSchemaSearch = false;
            RestrictedSchemaPageNumber = 1;
            RestrictedSchemaPageSize = 2;

            GroupId = groupId;

            ctRestrictedSchemaPage = GetCurrentRestrictedSchemaPage();
            ViewBag.RestrictedSchemaPage = ctRestrictedSchemaPage;

            return View(ctRestrictedSchema);
            //return View();
        }//end method

        public List<RestrictedSchema> GetCurrentRestrictedSchemaPage() {
            List<RestrictedSchema> list = null;
            using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetUsersPaged(RestrictedSchemaPageNumber, RestrictedSchemaPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of RestrictedSchema's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitRestrictedSchema(RestrictedSchema authTab) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
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

                ctRestrictedSchemaPage = GetCurrentRestrictedSchemaPage();
                ViewBag.RestrictedSchemaPage = ctRestrictedSchemaPage;

                ctRestrictedSchema = null;

                return View("Index", ctRestrictedSchema);
            }
            else {
                ctRestrictedSchemaPage = GetCurrentRestrictedSchemaPage();
                ViewBag.RestrictedSchemaPage = ctRestrictedSchemaPage;

                ctRestrictedSchema = authTab;
                return View("Index", ctRestrictedSchema);
            }

        }//end method

        public ActionResult DeleteRestrictedSchema(int authTabId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedSchema authTab = null;

            //delete RestrictedSchema
            using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
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

            ctRestrictedSchema = authTab;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedSchemaSetDataSource();
            ViewBag.RestrictedSchemaPage = ctRestrictedSchemaPage;
            ViewBag.RestrictedSchemaSearchStr = RestrictedSchemaSearchStr;

            return View("Index", ctRestrictedSchema);
        }//end method
        #endregion

        #region RestrictedSchemaRelatedPaging
        public List<AdHockey.Models.RestrictedSchema> ctRestrictedSchemaPage {
            get { return (List<RestrictedSchema>)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsRestrictedSchemaSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"] = value; }
        }//end method

        public String RestrictedSchemaSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int RestrictedSchemaPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int RestrictedSchemaPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<RestrictedSchema> GetRestrictedSchemaUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
                    return repo.GetUsersPaged(RestrictedSchemaPageNumber, RestrictedSchemaPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public List<RestrictedSchema> GetRestrictedSchemaSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchUsersPaged(RestrictedSchemaSearchStr, RestrictedSchemaPageNumber, RestrictedSchemaPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedSchemaUnserarchedCount() {
            try {
                using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedSchemaSearchCount(String searchStr) {
            try {
                using (RestrictedSchemaRepository repo = new RestrictedSchemaRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(RestrictedSchemaSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void RestrictedSchemaSetDataSource() {
            try {
                if (IsRestrictedSchemaSearch) {
                    ctRestrictedSchemaPage = GetRestrictedSchemaSearchedPage(RestrictedSchemaSearchStr, RestrictedSchemaPageNumber, RestrictedSchemaPageSize);
                }
                else {
                    ctRestrictedSchemaPage = GetRestrictedSchemaUnsearchedPage(RestrictedSchemaPageNumber, RestrictedSchemaPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctRestrictedSchemaPage = new List<RestrictedSchema>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public ActionResult RestrictedSchemaFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedSchemaPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RestrictedSchemaPageNumber != 1) {
                --RestrictedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedSchemaItems = -1;

            if (IsRestrictedSchemaSearch)
                numRestrictedSchemaItems = GetRestrictedSchemaSearchCount(RestrictedSchemaSearchStr);
            else
                numRestrictedSchemaItems = GetRestrictedSchemaUnserarchedCount();

            if (Math.Ceiling((decimal)numRestrictedSchemaItems / (decimal)RestrictedSchemaPageSize) >= RestrictedSchemaPageNumber + 1) {
                ++RestrictedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedSchemas = -1;

            if (IsRestrictedSchemaSearch)
                numRestrictedSchemas = GetRestrictedSchemaSearchCount(RestrictedSchemaSearchStr);
            else
                numRestrictedSchemas = GetRestrictedSchemaUnserarchedCount();

            RestrictedSchemaPageNumber = (numRestrictedSchemas / RestrictedSchemaPageSize) + ((numRestrictedSchemas / RestrictedSchemaPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedSchemaSearch = true;
            RestrictedSchemaSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedSchemaSearch = false;
            RestrictedSchemaSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace