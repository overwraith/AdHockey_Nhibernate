using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Configuration;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers
{
    public class ManageReportController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");
        private static readonly log4net.ILog useLog = log4net.LogManager.GetLogger("RollingUsageAppender");

        /// <summary>
        /// Stores the current report the controller is working with. 
        /// </summary>
        public Report ctReport {
            get {
                return (Report)System.Web.HttpContext.Current.Session["CT_REPORT"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_REPORT"] = value;
            }
        }//end property

        public Synopsis ctSynopsis {
            get {
                return (Synopsis)System.Web.HttpContext.Current.Session["CT_SYNOPSIS"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_SYNOPSIS"] = value;
            }
        }//end property

        /// <summary>
        /// Landing page. Set up variables for rest of page operation. 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //initialize new objects
            ctReport = new Report();
            ctSynopsis = new Synopsis();

            //set up report pagination
            IsReportSearch = false;
            ReportPageNumber = 1;
            ReportPageSize = 2;

            //get current page of reports
            ctReportPage = GetCurrentReportPage();
            ViewBag.ReportPage = ctReportPage;

            //set up synopsis pagination
            IsSynSearch = false;
            SynPageNumber = 1;
            SynPageSize = 5;

            //append a synopsis object for the current admin
            User admin = GetAdminUser(ConfigurationManager.AppSettings["AdminEmails"]);

            //add a default synopsis object
            Synopsis synopsis = new Synopsis() {
                RecommenderFirstName = admin.FirstName,
                RecommenderLastName = admin.LastName,
                BusinessCase = "Admin is adding report to reporting engine. ",
                DateCreated = DateTime.Now,
                ReccomenderTelephoneNum = admin.TelephoneNumber
            };

            //add synopsis to report and full list of synopsises
            ctReport.Synopses = new List<Synopsis>();
            ctReport.Synopses.Add(synopsis);

            ctSynPage = GetCurrentSynopsisPage();
            ViewBag.SynopsisPage = ctSynPage;

            return View(ctReport);
        }//end method

        /// <summary>
        /// Returns based on email address the current admin's user profile. 
        /// </summary>
        public AdHockey.Models.User GetAdminUser(String emailAddress) {
            try {
                using (UserRepository usrRepo = new UserRepository()) {
                    usrRepo.BeginTransaction();
                    return usrRepo.GetByEmail(emailAddress);
                }
            }
            catch (Exception ex) {
                log.Error(String.Format("Error getting user profile for admin {0}. ", emailAddress));
                throw ex;
            }
        }//end propert

        /// <summary>
        /// Gets the current report page based on page number and page size. 
        /// </summary>
        /// <returns></returns>
        public List<Report> GetCurrentReportPage() {
            List<Report> list = null;
            using (ReportRepository repo = new ReportRepository()) {
                repo.BeginTransaction();
                try {
                    list = repo.GetReportsPaged(ReportPageNumber, ReportPageSize).ToList();
                }
                catch (Exception ex) {
                    log.Error("Error getting page of reports. ", ex);
                    throw;
                }
            }

            return list;
        }//end method

        /// <summary>
        /// Gets the current page of synopses based on page number and page size. 
        /// </summary>
        /// <returns></returns>
        public List<Synopsis> GetCurrentSynopsisPage() {
            List<Synopsis> list = null;

            if (!IsSynSearch)
                list = GetSynopsisUnsearchedPage(SynPageNumber, SynPageSize);
            else
                list = GetSynopsisSearchedPage(SynSearchStr, SynPageNumber, SynPageSize);

            return list;
        }//end method

        /// <summary>
        /// Insert report into database. If model error return back to user. 
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public ActionResult SubmitReport(Report report) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                //insert report into database
                try {
                    using (ReportRepository repo = new ReportRepository()) {
                        //get this user
                        User user = null;
                        using (UserRepository usrRepo = new UserRepository()) {
                            usrRepo.BeginTransaction();
                            user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
                        }//end using

                        //log the user that created the report
                        useLog.Debug(String.Format("User {0} {1} created report {2} ", user.FirstName, user.LastName, report.ReportName));

                        //insert the report into db
                        repo.Insert(report);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error inserting report into database. ", ex);
                    throw;
                }

                ModelState.Clear();

                //return a blank synopsis page
                ctSynPage = new List<Synopsis>();
                ViewBag.Synopses = ctSynPage;

                ctReportPage = GetCurrentReportPage();
                ViewBag.ReportPage = ctReportPage;

                return View("Index");
            }
            else {
                //give the view back it's current synopsis

                //give view it's current Synopsis page
                ctSynPage = ctReport.Synopses.ToList();
                ViewBag.Synopses = GetSynopsisUnsearchedPage(SynPageNumber, SynPageSize);

                //give the view back it's current report
                ctReportPage = GetCurrentReportPage();
                //ViewBag.ReportPage = ;
                ctReport = report;
                return View("Index", ctReport);
            }
        }//end method

        /// <summary>
        /// Delete report from database. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        [Route("Index/{reportId}")]
        public ActionResult DeleteReport(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            using (ReportRepository repo = new ReportRepository()) {
                try {
                    repo.Delete(reportId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting report from database. ", ex);
                    repo.RollbackTransaction();
                    throw;
                }
            }

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AddSynopsis(Synopsis synopsis) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Report report = ctReport;

            //check if list is unassigned
            if (report.Synopses == null)
                report.Synopses = new List<Synopsis>();

            //get current admin by email address
            User admin = null;
            if (ConfigurationManager.AppSettings["AdminEmails"].Split(';')
                .Contains(Session["USER_NAME"].ToString()))
                admin = GetAdminUser(Session["USER_NAME"].ToString());

            //add implementer information, the admin
            synopsis.DateCreated = DateTime.Now;
            synopsis.ImplementerFirstName = admin.FirstName;
            synopsis.ImplementerLastName = admin.LastName;
            synopsis.ImplementerTelephoneNum = admin.TelephoneNumber;

            //add field to profile
            report.Synopses.Add(synopsis);

            return RedirectToAction("SetDataSource");
        }//end method

        //public ActionResult AddSynopsis([Bind(Include = "RecommenderFirstName,RecommenderLastName,ReccomenderTelephoneNum,ImplementerFirstName,ImplementerLastName,ImplementerTelephoneNum,DateCreated,BusinessCase,ReportName,Description,Sql")]ReportSynopsisViewModel something) {
        //    //check that user is logged in
        //    if (Session["USER_NAME"] == null)
        //        return RedirectToAction("index", "Login");

        //    if (ModelState.IsValid) {
        //        Synopsis synopsis = something.Synopsis;

        //        //get current admin by email address
        //        User admin = GetAdminUser(ConfigurationManager.AppSettings["AdminEmails"]);

        //        //add implementer information, the admin
        //        synopsis.DateCreated = DateTime.Now;
        //        synopsis.ImplementerFirstName = admin.FirstName;
        //        synopsis.ImplementerLastName = admin.LastName;
        //        synopsis.ImplementerTelephoneNum = admin.TelephoneNumber;

        //        ctReport.Synopses.Add(synopsis);

        //        //give view it's current Synopsis page
        //        ctSynPage = new List<Synopsis>();
        //        ViewBag.SynopsisPage = ctSynPage;

        //        ModelState.Clear();
        //        return View("Index");
        //    }
        //    else {
        //        //give the view back it's current synopsis

        //        //give view it's current Synopsis page
        //        ViewBag.SynopsisPage = GetCurrentSynopsisPage();

        //        //assign reports
        //        ctReport = something.Report;
        //        ctSynopsis = something.Synopsis;

        //        ViewBag.ReportPage = ctReportPage;
        //        return View("Index", new ReportSynopsisViewModel() { Report = ctReport, Synopsis = ctSynopsis });
        //    }
        //}//end method

        #region GlobalPagingRelated
        /// <summary>
        /// All pagination set data source methods need placed in this global method. 
        /// </summary>
        /// <returns></returns>
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ReportSetDataSource();

            ViewBag.ReportPage = ctReportPage;
            ViewBag.ReportSearchStr = ReportSearchStr;

            SynSetDataSource();

            ViewBag.SynopsisPage = ctSynPage;

            return View("Index", ctReport);
        }
        #endregion

        /// <summary>
        /// All paging related stuff, uses database and repositories. Database paging. 
        /// </summary>
        #region ReportRelatedPaging
        public List<Report> ctReportPage {
            get {
                return (List<Report>)System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"] = value;
            }
        }//end property

        /// <summary>
        /// Used to determine whether to toggle into a searched mode or a non searched mode. 
        /// </summary>
        public bool IsReportSearch {
            get {
                return (bool)System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"];
            }
            set {
                System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"] = value;
            }
        }//end property

        /// <summary>
        /// Stores the current search string for usage between method calls. 
        /// </summary>
        public String ReportSearchStr {
            get {
                return (String)System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"];
            }
            set {
                System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"] = value;
            }
        }//end property

        /// <summary>
        /// Current page number on report page. 
        /// </summary>
        public int ReportPageNumber {
            get {
                return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"];
            }
            set {
                System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"] = value;
            }
        }//end property

        /// <summary>
        /// Current Page Size. 
        /// </summary>
        public int ReportPageSize {
            get {
                return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"];
            }
            set {
                System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"] = value;
            }
        }//end property

        /// <summary>
        /// Gets reports in an unsearched context. (pagination)
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Report> GetReportUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetReportsPaged(ReportPageNumber, ReportPageSize);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }
        }//end method

        /// <summary>
        /// Gets reports in a searched context. (pagination)
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Report> GetReportSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.SearchReportsPaged(ReportSearchStr, ReportPageNumber, ReportPageSize);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of reports. ", ex);
                throw;
            }
        }//end method

        /// <summary>
        /// Gets a count of the number of reports in the database. 
        /// </summary>
        /// <returns></returns>
        public int GetReportUnsearchedCount() {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetNumReports();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of reports. ", ex);
                throw;
            }
        }//end method

        /// <summary>
        /// Gets the number of search results retrieved from a search string. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public int GetReportSearchCount(String searchStr) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetTotalNumSearchResults(ReportSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of search results. ", ex);
                throw;
            }
        }//end method

        /// <summary>
        /// Retrieves the current page regaurdless of whether is in search mode or unsearched mode. 
        /// </summary>
        public void ReportSetDataSource() {
            try {
                if (IsReportSearch)
                    ctReportPage = GetReportSearchedPage(ReportSearchStr, ReportPageNumber, ReportPageSize);
                else
                    ctReportPage = GetReportUnsearchedPage(ReportPageNumber, ReportPageSize);
            }
            catch (ArgumentException) {
                ctReportPage = new List<Report>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }
        }//end method

        /// <summary>
        /// Retrieves the first page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ReportPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Retrieves the previous page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine if we are on the first page
            if (ReportPageNumber != 1) {
                --ReportPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Retrieves the next page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numReports = -1;

            if(IsReportSearch)
                numReports = GetReportSearchCount(ReportSearchStr);
            else
                numReports = GetReportUnsearchedCount();

            if (Math.Ceiling(((decimal)numReports / (decimal)ReportPageSize)) >= ReportPageNumber + 1) {
                ++ReportPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Retrieves the last page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numReports = -1;

            if (IsReportSearch)
                numReports = GetReportSearchCount(ReportSearchStr);
            else
                numReports = GetReportUnsearchedCount();

            ReportPageNumber = (numReports / ReportPageSize) + ((numReports / ReportPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Sets IsSearch flag to true and retrieves the search string. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsReportSearch = true;
            ReportSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Clear Search box and set the IsSearch flag to false. 
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsReportSearch = false;
            ReportSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        /// <summary>
        /// All paging related stuff, uses database and repositories. Database paging. 
        /// </summary>
        #region SynopsisRelatedPaging
        public List<Synopsis> ctSynPage {
            get {
                return (List<Synopsis>)System.Web.HttpContext.Current.Session["CT_SYNOPSIS_PAGE"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_SYNOPSIS_PAGE"] = value;
            }
        }//end property

        /// <summary>
        /// Used to determine whether to toggle into a searched mode or a non searched mode. 
        /// </summary>
        public bool IsSynSearch {
            get {
                return (bool)System.Web.HttpContext.Current.Session["IS_SYNOPSIS_SEARCH"];
            }
            set {
                System.Web.HttpContext.Current.Session["IS_SYNOPSIS_SEARCH"] = value;
            }
        }//end property

        /// <summary>
        /// Stores the current search string for usage between method calls. 
        /// </summary>
        public String SynSearchStr {
            get {
                return (String)System.Web.HttpContext.Current.Session["SYNOPSIS_SEARCH_STRING"];
            }
            set {
                System.Web.HttpContext.Current.Session["SYNOPSIS_SEARCH_STRING"] = value;
            }
        }//end property

        /// <summary>
        /// Current page number on report page. 
        /// </summary>
        public int SynPageNumber {
            get {
                return (int)System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_NUMBER"];
            }
            set {
                System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_NUMBER"] = value;
            }
        }//end property

        /// <summary>
        /// Current Page Size. 
        /// </summary>
        public int SynPageSize {
            get {
                return (int)System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_SIZE"];
            }
            set {
                System.Web.HttpContext.Current.Session["SYNOPSIS_PAGE_SIZE"] = value;
            }
        }//end property

        /// <summary>
        /// Gets reports in an unsearched context. (pagination)
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Synopsis> GetSynopsisUnsearchedPage(int pageNumber, int pageSize) {
            return ctReport.Synopses
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }//end method

        /// <summary>
        /// Gets reports in a searched context. (pagination)
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Synopsis> GetSynopsisSearchedPage(String searchStr, int pageNumber, int pageSize) {
            String[] toks = searchStr.ToLower().Split();
            List<Synopsis> synopses = new List<Synopsis>();

            foreach (var tok in toks) {
                var list1 = ctReport.Synopses
                .Where(syn => new Regex(String.Format(".*{0}.*", tok), RegexOptions.IgnoreCase).Match(syn.RecommenderFirstName.ToLower()).Success)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

                var list2 = ctReport.Synopses
                .Where(syn => new Regex(String.Format(".*{0}.*", tok), RegexOptions.IgnoreCase).Match(syn.RecommenderLastName.ToLower()).Success)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

                synopses.AddRange(list1);
                synopses.AddRange(list2);
            }
            return synopses;
        }//end method

        /// <summary>
        /// Gets a count of the number of reports in the database. 
        /// </summary>
        /// <returns></returns>
        public int GetSynopsisUnsearchedCount() {
            return ctReport.Synopses.Count();
        }//end method

        /// <summary>
        /// Gets the number of search results retrieved from a search string. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public int GetSynopsisSearchCount(String searchStr) {
            String[] toks = searchStr.ToLower().Split();
            List<Synopsis> synopses = new List<Synopsis>();

            foreach (var tok in toks) {
                var list1 = ctReport.Synopses
                .Where(syn => new Regex(String.Format(".*{0}.*", tok), RegexOptions.IgnoreCase).Match(syn.RecommenderFirstName.ToLower()).Success)
                .Skip((SynPageNumber - 1) * SynPageSize)
                .Take(SynPageSize)
                .ToList();

                var list2 = ctReport.Synopses
                .Where(syn => new Regex(String.Format(".*{0}.*", tok), RegexOptions.IgnoreCase).Match(syn.RecommenderLastName.ToLower()).Success)
                .Skip((SynPageNumber - 1) * SynPageSize)
                .Take(SynPageSize)
                .ToList();

                synopses.AddRange(list1);
                synopses.AddRange(list2);
            }
            return synopses.Count();
        }//end method

        /// <summary>
        /// Retrieves the current page regaurdless of whether is in search mode or unsearched mode. 
        /// </summary>
        public void SynSetDataSource() {
            try {
                if (IsSynSearch)
                    ctSynPage = GetSynopsisSearchedPage(SynSearchStr, SynPageNumber, SynPageSize);
                else
                    ctSynPage = GetSynopsisUnsearchedPage(SynPageNumber, SynPageSize);
            }
            catch (ArgumentException) {
                ctSynPage = new List<Synopsis>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }
        }//end method

        /// <summary>
        /// Retrieves the first page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult SynFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            SynPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Retrieves the previous page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult SynPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine if we are on the first page
            if (SynPageNumber != 1) {
                --SynPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Retrieves the next page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult SynNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numSynopsiss = -1;

            if (IsSynSearch)
                numSynopsiss = GetSynopsisSearchCount(SynSearchStr);
            else
                numSynopsiss = GetSynopsisUnsearchedCount();

            if (Math.Ceiling(((decimal)numSynopsiss / (decimal)SynPageSize)) >= SynPageNumber + 1) {
                ++SynPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Retrieves the last page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult SynLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numSynopsiss = -1;

            if (IsSynSearch)
                numSynopsiss = GetSynopsisSearchCount(SynSearchStr);
            else
                numSynopsiss = GetSynopsisUnsearchedCount();

            SynPageNumber = (numSynopsiss / SynPageSize) + ((numSynopsiss / SynPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Sets IsSearch flag to true and retrieves the search string. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public ActionResult ExecuteSynSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsSynSearch = true;
            SynSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        /// <summary>
        /// Clear Search box and set the IsSearch flag to false. 
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearSynSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsSynSearch = false;
            SynSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace