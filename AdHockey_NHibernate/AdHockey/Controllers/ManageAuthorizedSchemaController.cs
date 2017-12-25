/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageAuthorizedSchemaController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public AuthorizedSchema ctAuthorizedSchema {
            get { return (AuthorizedSchema)System.Web.HttpContext.Current.Session["CT_AUTH_SCHEMA"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTH_SCHEMA"] = value; }
        }//end property

        public int GroupId {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_ID"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_ID"] = value; }
        }//end property

        // GET: ManageAuthorizedSchema
        public ActionResult Index(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedSchemaSearch = false;
            AuthorizedSchemaPageNumber = 1;
            AuthorizedSchemaPageSize = 2;

            GroupId = groupId;

            ctAuthorizedSchemaPage = GetCurrentAuthorizedSchemaPage();
            ViewBag.AuthorizedSchemaPage = ctAuthorizedSchemaPage;

            return View(ctAuthorizedSchema);
            //return View();
        }//end method

        public List<AuthorizedSchema> GetCurrentAuthorizedSchemaPage() {
            List<AuthorizedSchema> list = null;
            using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetUsersPaged(AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of AuthorizedSchema's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitAuthorizedSchema(AuthorizedSchema authTab) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
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

                ctAuthorizedSchemaPage = GetCurrentAuthorizedSchemaPage();
                ViewBag.AuthorizedSchemaPage = ctAuthorizedSchemaPage;

                ctAuthorizedSchema = null;

                return View("Index", ctAuthorizedSchema);
            }
            else {
                ctAuthorizedSchemaPage = GetCurrentAuthorizedSchemaPage();
                ViewBag.AuthorizedSchemaPage = ctAuthorizedSchemaPage;

                ctAuthorizedSchema = authTab;
                return View("Index", ctAuthorizedSchema);
            }

        }//end method

        public ActionResult DeleteAuthorizedSchema(int authTabId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedSchema authTab = null;

            //delete AuthorizedSchema
            using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
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

            ctAuthorizedSchema = authTab;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedSchemaSetDataSource();
            ViewBag.AuthorizedSchemaPage = ctAuthorizedSchemaPage;
            ViewBag.AuthorizedSchemaSearchStr = AuthorizedSchemaSearchStr;

            return View("Index", ctAuthorizedSchema);
        }//end method
        #endregion

        #region AuthorizedSchemaRelatedPaging
        public List<AdHockey.Models.AuthorizedSchema> ctAuthorizedSchemaPage {
            get { return (List<AuthorizedSchema>)System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTHORIZED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsAuthorizedSchemaSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTHORIZED_TABLE_SEARCH"] = value; }
        }//end method

        public String AuthorizedSchemaSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int AuthorizedSchemaPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int AuthorizedSchemaPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTHORIZED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<AuthorizedSchema> GetAuthorizedSchemaUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
                    return repo.GetUsersPaged(AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public List<AuthorizedSchema> GetAuthorizedSchemaSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchSchemasPaged(AuthorizedSchemaSearchStr, AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedSchemaUnserarchedCount() {
            try {
                using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of authorized table. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedSchemaSearchCount(String searchStr) {
            try {
                using (AuthorizedSchemaRepository repo = new AuthorizedSchemaRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(AuthorizedSchemaSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void AuthorizedSchemaSetDataSource() {
            try {
                if (IsAuthorizedSchemaSearch) {
                    ctAuthorizedSchemaPage = GetAuthorizedSchemaSearchedPage(AuthorizedSchemaSearchStr, AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize);
                }
                else {
                    ctAuthorizedSchemaPage = GetAuthorizedSchemaUnsearchedPage(AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctAuthorizedSchemaPage = new List<AuthorizedSchema>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of authorized table. ", ex);
                throw;
            }
        }//end method

        public ActionResult AuthorizedSchemaFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedSchemaPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedSchemaPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (AuthorizedSchemaPageNumber != 1) {
                --AuthorizedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedSchemaNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedSchemaItems = -1;

            if (IsAuthorizedSchemaSearch)
                numAuthorizedSchemaItems = GetAuthorizedSchemaSearchCount(AuthorizedSchemaSearchStr);
            else
                numAuthorizedSchemaItems = GetAuthorizedSchemaUnserarchedCount();

            if (Math.Ceiling((decimal)numAuthorizedSchemaItems / (decimal)AuthorizedSchemaPageSize) >= AuthorizedSchemaPageNumber + 1) {
                ++AuthorizedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedSchemaLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedSchemas = -1;

            if (IsAuthorizedSchemaSearch)
                numAuthorizedSchemas = GetAuthorizedSchemaSearchCount(AuthorizedSchemaSearchStr);
            else
                numAuthorizedSchemas = GetAuthorizedSchemaUnserarchedCount();

            AuthorizedSchemaPageNumber = (numAuthorizedSchemas / AuthorizedSchemaPageSize) + ((numAuthorizedSchemas / AuthorizedSchemaPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedSchemaSearch = true;
            AuthorizedSchemaSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedSchemaSearch = false;
            AuthorizedSchemaSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace