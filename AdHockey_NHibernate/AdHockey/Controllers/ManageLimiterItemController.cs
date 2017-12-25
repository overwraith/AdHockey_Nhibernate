using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageLimiterItemController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public LimiterItem ctLimiterItem {
            get { return (LimiterItem)System.Web.HttpContext.Current.Session["CT_LIMITER_ITEM"]; }
            set { System.Web.HttpContext.Current.Session["CT_LIMITER_ITEM"] = value; }
        }//end property

        public int ReportId {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_ID"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_ID"] = value; }
        }//end property

        // GET: ManageLimiterItem
        public ActionResult Index(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsLimiterItemSearch = false;
            LimiterItemPageNumber = 1;
            LimiterItemPageSize = 2;

            ReportId = reportId;

            ctLimiterItemPage = GetCurrentLimiterItemPage();
            ViewBag.LimiterItemPage = ctLimiterItemPage;

            return View(ctLimiterItem);
            //return View();
        }//end method

        public List<LimiterItem> GetCurrentLimiterItemPage() {
            List<LimiterItem> list = null;
            using (LimiterItemRepository repo = new LimiterItemRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetUsersPaged(LimiterItemPageNumber, LimiterItemPageSize, ReportId);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of Users. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitUser(LimiterItem User) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (LimiterItemRepository repo = new LimiterItemRepository()) {
                        repo.BeginTransaction();

                        Report report = null;
                        using (ReportRepository rptRepo = new ReportRepository()) {
                            rptRepo.BeginTransaction();
                            report = rptRepo.GetById(ReportId);
                        }

                        User.ReportId = ReportId;
                        User.Report = report;
                        repo.Insert(User);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error Submitting User. ", ex);
                    throw;
                }
                ModelState.Clear();

                return View("Index", ctLimiterItem);
            }
            else {
                ctLimiterItemPage = GetCurrentLimiterItemPage();
                ViewBag.LimiterItemPage = ctLimiterItemPage;

                ctLimiterItem = User;
                return View("Index", ctLimiterItem);
            }

        }//end method

        public ActionResult DeleteLimiterItem(int LimiterItemId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            LimiterItem LimiterItem = null;

            //delete LimiterItem
            using (LimiterItemRepository repo = new LimiterItemRepository()) {
                try {
                    repo.BeginTransaction();
                    LimiterItem = repo.GetById(LimiterItemId);
                    repo.Delete(LimiterItemId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting limiter item. ", ex);
                    throw;
                }
            }

            ctLimiterItem = LimiterItem;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            LimiterItemSetDataSource();
            ViewBag.LimiterItemPage = ctLimiterItemPage;
            ViewBag.LimiterItemSearchStr = LimiterItemSearchStr;

            return View("Index", ctLimiterItem);
        }//end method
        #endregion

        #region LimiterItemRelatedPaging
        public List<AdHockey.Models.LimiterItem> ctLimiterItemPage {
            get { return (List<LimiterItem>)System.Web.HttpContext.Current.Session["CT_LIMITER_ITEM_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_LIMITER_ITEM_PAGE"] = value; }
        }//end method

        public bool IsLimiterItemSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_LIMITER_ITEM_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_LIMITER_ITEM_SEARCH"] = value; }
        }//end method

        public String LimiterItemSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["LIMITER_ITEM_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["LIMITER_ITEM_SEARCH_STRING"] = value; }
        }//end method

        public int LimiterItemPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["LIMITER_ITEM_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["LIMITER_ITEM_PAGE_NUMBER"] = value; }
        }//end method

        public int LimiterItemPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["LIMITER_ITEM_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["LIMITER_ITEM_PAGE_SIZE"] = value; }
        }//end method

        public List<LimiterItem> GetLimiterItemUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (LimiterItemRepository repo = new LimiterItemRepository()) {
                    return repo.GetUsersPaged(LimiterItemPageNumber, LimiterItemPageSize, ReportId).ToList();
                }
            }
            catch (Exception) {
                throw;
            }
        }//end method

        public List<LimiterItem> GetLimiterItemSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (LimiterItemRepository repo = new LimiterItemRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchUsersPaged(LimiterItemSearchStr, LimiterItemPageNumber, LimiterItemPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Users. ", ex);
                throw;
            }
        }//end method

        public int GetLimiterItemUnserarchedCount() {
            try {
                using (LimiterItemRepository repo = new LimiterItemRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of Users. ", ex);
                throw;
            }
        }//end method

        public int GetLimiterItemSearchCount(String searchStr) {
            try {
                using (LimiterItemRepository repo = new LimiterItemRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(LimiterItemSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void LimiterItemSetDataSource() {
            try {
                if (IsLimiterItemSearch) {
                    ctLimiterItemPage = GetLimiterItemSearchedPage(LimiterItemSearchStr, LimiterItemPageNumber, LimiterItemPageSize);
                }
                else {
                    ctLimiterItemPage = GetLimiterItemUnsearchedPage(LimiterItemPageNumber, LimiterItemPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctLimiterItemPage = new List<LimiterItem>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of Limiter Items.  ", ex);
                throw;
            }
        }//end method

        public ActionResult LimiterItemFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            LimiterItemPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult LimiterItemPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (LimiterItemPageNumber != 1) {
                --LimiterItemPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult LimiterItemNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numLimiterItemItems = -1;

            if (IsLimiterItemSearch)
                numLimiterItemItems = GetLimiterItemSearchCount(LimiterItemSearchStr);
            else
                numLimiterItemItems = GetLimiterItemUnserarchedCount();

            if (Math.Ceiling((decimal)numLimiterItemItems / (decimal)LimiterItemPageSize) >= LimiterItemPageNumber + 1) {
                ++LimiterItemPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult LimiterItemLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numLimiterItems = -1;

            if (IsLimiterItemSearch)
                numLimiterItems = GetLimiterItemSearchCount(LimiterItemSearchStr);
            else
                numLimiterItems = GetLimiterItemUnserarchedCount();

            LimiterItemPageNumber = (numLimiterItems / LimiterItemPageSize) + ((numLimiterItems / LimiterItemPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsLimiterItemSearch = true;
            LimiterItemSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsLimiterItemSearch = false;
            LimiterItemSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace