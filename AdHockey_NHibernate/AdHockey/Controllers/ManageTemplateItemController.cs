using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageTemplateItemController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public TemplateItem ctTemplateItem {
            get { return (TemplateItem)System.Web.HttpContext.Current.Session["CT_TEMPLATE_ITEM"]; }
            set { System.Web.HttpContext.Current.Session["CT_TEMPLATE_ITEM"] = value; }
        }//end property

        public int ReportId {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_ID"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_ID"] = value; }
        }//end property

        // GET: ManageTemplateItem
        public ActionResult Index(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsTemplateItemSearch = false;
            TemplateItemPageNumber = 1;
            TemplateItemPageSize = 2;

            ReportId = reportId;

            ctTemplateItemPage = GetCurrentTemplateItemPage();
            ViewBag.TemplateItemPage = ctTemplateItemPage;

            return View(ctTemplateItem);
            //return View();
        }//end method

        public List<TemplateItem> GetCurrentTemplateItemPage() {
            List<TemplateItem> list = null;
            using (TemplateItemRepository repo = new TemplateItemRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetTemplatesPaged(TemplateItemPageNumber, TemplateItemPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of TemplateItem's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitUser(TemplateItem User) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (TemplateItemRepository repo = new TemplateItemRepository()) {
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

                ctTemplateItemPage = GetCurrentTemplateItemPage();
                ViewBag.TemplateItemPage = ctTemplateItemPage;

                ctTemplateItem = User;

                return View("Index", ctTemplateItem);
            }
            else {
                ctTemplateItemPage = GetCurrentTemplateItemPage();
                ViewBag.TemplateItemPage = ctTemplateItemPage;

                ctTemplateItem = User;
                return View("Index", ctTemplateItem);
            }

        }//end method

        public ActionResult DeleteTemplateItem(int TemplateItemId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            TemplateItem TemplateItem = null;

            //delete TemplateItem
            using (TemplateItemRepository repo = new TemplateItemRepository()) {
                try {
                    repo.BeginTransaction();
                    TemplateItem = repo.GetById(TemplateItemId);
                    repo.Delete(TemplateItemId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting User. ", ex);
                    throw;
                }
            }

            ctTemplateItem = TemplateItem;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            TemplateItemSetDataSource();
            ViewBag.TemplateItemPage = ctTemplateItemPage;
            ViewBag.TemplateItemSearchStr = TemplateItemSearchStr;

            return View("Index", ctTemplateItem);
        }//end method
        #endregion

        #region TemplateItemRelatedPaging
        public List<AdHockey.Models.TemplateItem> ctTemplateItemPage {
            get { return (List<TemplateItem>)System.Web.HttpContext.Current.Session["CT_TEMPLATE_ITEM_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_TEMPLATE_ITEM_PAGE"] = value; }
        }//end method

        public bool IsTemplateItemSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_TEMPLATE_ITEM_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_TEMPLATE_ITEM_SEARCH"] = value; }
        }//end method

        public String TemplateItemSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["TEMPLATE_ITEM_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["TEMPLATE_ITEM_SEARCH_STRING"] = value; }
        }//end method

        public int TemplateItemPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["TEMPLATE_ITEM_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["TEMPLATE_ITEM_PAGE_NUMBER"] = value; }
        }//end method

        public int TemplateItemPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["TEMPLATE_ITEM_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["TEMPLATE_ITEM_PAGE_SIZE"] = value; }
        }//end method

        public List<TemplateItem> GetTemplateItemUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (TemplateItemRepository repo = new TemplateItemRepository()) {
                    return repo.GetTemplatesPaged(TemplateItemPageNumber, TemplateItemPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of Users. ", ex);
                throw;
            }
        }//end method

        public List<TemplateItem> GetTemplateItemSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (TemplateItemRepository repo = new TemplateItemRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchTemplatesPaged(TemplateItemSearchStr, TemplateItemPageNumber, TemplateItemPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of Users. ", ex);
                throw;
            }
        }//end method

        public int GetTemplateItemUnserarchedCount() {
            try {
                using (TemplateItemRepository repo = new TemplateItemRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumTemplates();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of Users. ", ex);
                throw;
            }
        }//end method

        public int GetTemplateItemSearchCount(String searchStr) {
            try {
                using (TemplateItemRepository repo = new TemplateItemRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(TemplateItemSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void TemplateItemSetDataSource() {
            try {
                if (IsTemplateItemSearch) {
                    ctTemplateItemPage = GetTemplateItemSearchedPage(TemplateItemSearchStr, TemplateItemPageNumber, TemplateItemPageSize);
                }
                else {
                    ctTemplateItemPage = GetTemplateItemUnsearchedPage(TemplateItemPageNumber, TemplateItemPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctTemplateItemPage = new List<TemplateItem>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of Users. ", ex);
                throw;
            }
        }//end method

        public ActionResult TemplateItemFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            TemplateItemPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TemplateItemPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (TemplateItemPageNumber != 1) {
                --TemplateItemPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TemplateItemNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numTemplateItemItems = -1;

            if (IsTemplateItemSearch)
                numTemplateItemItems = GetTemplateItemSearchCount(TemplateItemSearchStr);
            else
                numTemplateItemItems = GetTemplateItemUnserarchedCount();

            if (Math.Ceiling((decimal)numTemplateItemItems / (decimal)TemplateItemPageSize) >= TemplateItemPageNumber + 1) {
                ++TemplateItemPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TemplateItemLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numTemplateItems = -1;

            if (IsTemplateItemSearch)
                numTemplateItems = GetTemplateItemSearchCount(TemplateItemSearchStr);
            else
                numTemplateItems = GetTemplateItemUnserarchedCount();

            TemplateItemPageNumber = (numTemplateItems / TemplateItemPageSize) + ((numTemplateItems / TemplateItemPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsTemplateItemSearch = true;
            TemplateItemSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsTemplateItemSearch = false;
            TemplateItemSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace