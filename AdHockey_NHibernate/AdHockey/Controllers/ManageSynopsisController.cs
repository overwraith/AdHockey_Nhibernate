using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageSynopsisController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public Synopsis ctSynopsis {
            get { return (Synopsis)System.Web.HttpContext.Current.Session["CT_SYNOPSIS"]; }
            set { System.Web.HttpContext.Current.Session["CT_SYNOPSIS"] = value; }
        }//end property

        public int ReportId {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_ID"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_ID"] = value; }
        }//end property

        // GET: ManageSynopsis
        public ActionResult Index(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsSynopsisSearch = false;
            SynopsisPageNumber = 1;
            SynopsisPageSize = 2;

            ReportId = reportId;

            ctSynopsisPage = GetCurrentSynopsisPage();
            ViewBag.SynopsisPage = ctSynopsisPage;

            return View(ctSynopsis);
            //return View();
        }//end method

        public List<Synopsis> GetCurrentSynopsisPage() {
            List<Synopsis> list = null;
            using (SynopsisRepository repo = new SynopsisRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetSynopsisPaged(ReportId, SynopsisPageNumber, SynopsisPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of synopsises. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitSynopsis(Synopsis synopsis) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (SynopsisRepository repo = new SynopsisRepository()) {
                        repo.BeginTransaction();

                        Report report = null;
                        using (ReportRepository rptRepo = new ReportRepository()) {
                            rptRepo.BeginTransaction();
                            report = rptRepo.GetById(ReportId);
                        }

                        synopsis.ReportId = ReportId;
                        synopsis.Report = report;
                        synopsis.DateCreated = DateTime.Now;
                        repo.Insert(synopsis);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error Submitting Synopsis. ", ex);
                    throw;
                }
                ModelState.Clear();

                return View("Index", ctSynopsis);
            }
            else {
                ctSynopsisPage = GetCurrentSynopsisPage();
                ViewBag.SynopsisPage = ctSynopsisPage;

                ctSynopsis = synopsis;
                return View("Index", ctSynopsis);
            }

        }//end method

        public ActionResult DeleteSynopsis(int synopsisId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Synopsis Synopsis = null;

            //delete Synopsis
            using (SynopsisRepository repo = new SynopsisRepository()) {
                try {
                    repo.BeginTransaction();
                    Synopsis = repo.GetById(synopsisId);
                    repo.Delete(synopsisId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting synopsis. ", ex);
                    throw;
                }
            }

            ctSynopsis = Synopsis;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            SynopsisSetDataSource();
            ViewBag.SynopsisPage = ctSynopsisPage;
            ViewBag.SynopsisSearchStr = SynopsisSearchStr;

            return View("Index", ctSynopsis);
        }//end method
        #endregion

        #region SynopsisRelatedPaging
        public List<AdHockey.Models.Synopsis> ctSynopsisPage {
            get { return (List<Synopsis>)System.Web.HttpContext.Current.Session["CT_SYNOPSIS_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_SYNOPSIS_PAGE"] = value; }
        }//end method

        public bool IsSynopsisSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_SYNOPSIS_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_SYNOPSIS_SEARCH"] = value; }
        }//end method

        public String SynopsisSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["SYNOPSIS_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["SYNOPSIS_SEARCH_STRING"] = value; }
        }//end method

        public int SynopsisPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_NUMBER"] = value; }
        }//end method

        public int SynopsisPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_SIZE"] = value; }
        }//end method

        public List<Synopsis> GetSynopsisUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (SynopsisRepository repo = new SynopsisRepository()) {
                    return repo.GetSynopsisPaged(SynopsisPageNumber, SynopsisPageSize, ReportId).ToList();
                }
            }
            catch (Exception) {
                throw;
            }
        }//end method

        public List<Synopsis> GetSynopsisSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (SynopsisRepository repo = new SynopsisRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchSynopsisPaged(SynopsisSearchStr, SynopsisPageNumber, SynopsisPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of synopsises. ", ex);
                throw;
            }
        }//end method

        public int GetSynopsisUnserarchedCount() {
            try {
                using (SynopsisRepository repo = new SynopsisRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumSynopsises();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of synopsises. ", ex);
                throw;
            }
        }//end method

        public int GetSynopsisSearchCount(String searchStr) {
            try {
                using (SynopsisRepository repo = new SynopsisRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(SynopsisSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void SynopsisSetDataSource() {
            try {
                if (IsSynopsisSearch) {
                    ctSynopsisPage = GetSynopsisSearchedPage(SynopsisSearchStr, SynopsisPageNumber, SynopsisPageSize);
                }
                else {
                    ctSynopsisPage = GetSynopsisUnsearchedPage(SynopsisPageNumber, SynopsisPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctSynopsisPage = new List<Synopsis>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of Synopsises.  ", ex);
                throw;
            }
        }//end method

        public ActionResult SynopsisFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            SynopsisPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SynopsisPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (SynopsisPageNumber != 1) {
                --SynopsisPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SynopsisNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numSynopsisItems = -1;

            if (IsSynopsisSearch)
                numSynopsisItems = GetSynopsisSearchCount(SynopsisSearchStr);
            else
                numSynopsisItems = GetSynopsisUnserarchedCount();

            if (Math.Ceiling((decimal)numSynopsisItems / (decimal)SynopsisPageSize) >= SynopsisPageNumber + 1) {
                ++SynopsisPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SynopsisLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numSynopsiss = -1;

            if (IsSynopsisSearch)
                numSynopsiss = GetSynopsisSearchCount(SynopsisSearchStr);
            else
                numSynopsiss = GetSynopsisUnserarchedCount();

            SynopsisPageNumber = (numSynopsiss / SynopsisPageSize) + ((numSynopsiss / SynopsisPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsSynopsisSearch = true;
            SynopsisSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsSynopsisSearch = false;
            SynopsisSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace