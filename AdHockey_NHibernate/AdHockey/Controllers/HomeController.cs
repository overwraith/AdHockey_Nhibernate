using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Net.Mime;
using System.Diagnostics;

using Newtonsoft.Json;
using AdHockey.Models;
using AdHockey.Repositories;
using AdHockey.Utilities;
using System.Net;

namespace AdHockey.Controllers {
    public class HomeController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");
        private static readonly log4net.ILog useLog = log4net.LogManager.GetLogger("RollingUsageAppender");

        public Report ctReport {
            get {
                return (Report)System.Web.HttpContext.Current.Session["CT_REPORT"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_REPORT"] = value;
            }
        }//end property

        /// <summary>
        /// Method executed when user navigates to page. 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ModelState.Clear();
            return View(ctReport);
        }//end method

        /// <summary>
        /// Get names of reports. 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetReportNames() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //retrieve report Users from the database
            List<String> names = null;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    names = repo.GetReportNames();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting report names. ", ex);
            }

            //serialize Users to json
            JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};
            String json = JsonConvert.SerializeObject(names, settings);

            return Content(json, "application/json");
        }//end method

        /// <summary>
        /// Get current page of reports. 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCurrentReportPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //retrieve reports from database
            List<Report> reports = null;
            using (ReportRepository repo = new ReportRepository()) {
                try {
                    reports = repo.GetReportsPaged(ReportPageNumber, ReportPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of reports. ", ex);
                    throw;
                }
            }

            //serialize reports to json
            JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            String json = JsonConvert.SerializeObject(reports, settings);

            return Content(json, "application/json");
        }//end method

        /// <summary>
        /// Get a single report according to id. 
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public ActionResult GetReport(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //retrieve report Users from the database
            Report report = null;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    report = repo.GetById(reportId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting report. ", ex);
            }

            //serialize Users to json
            JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            String json = JsonConvert.SerializeObject(report, settings);

            return Content(json, "application/json");
        }//end method

        /// <summary>
        /// Execute the currently selected report. 
        /// </summary>
        /// <param name="jsonReport"></param>
        /// <returns></returns>
        public ActionResult ExecuteReport(String jsonReport) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");
            
            //re-hydrate Users from web browser
            Report reportInput = JsonConvert.DeserializeObject<Report>(jsonReport);

            Report report = null;
            using (ReportRepository rptRepo = new ReportRepository()) {
                rptRepo.BeginTransaction();
                report = rptRepo.GetById(reportInput.ReportId);
            }

            //determine whether user has access to report 
            bool hasAccess = HasAccess(report);

            if (hasAccess) {
                int reportId = reportInput.ReportId;

                //get this user
                User user = null;
                using (UserRepository usrRepo = new UserRepository()) {
                    usrRepo.BeginTransaction();
                    user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
                }//end using

                String sessionId = HttpContext.Session.SessionID;
                using (ReportRepository repo = new ReportRepository()) {
                    //get data from database
                    DataSet ds = new DataSet();
                    ds.DataSetName = "export";
                    DataTable dt = null;

                    try {
                        dt = reportInput.ExecuteReport(user.UserId, sessionId);
                    }
                    catch (UnauthorizedAccessException ex) {
                        log.Debug(ex.Message, ex);
                        throw;
                    }

                    dt.TableName = "export";
                    ds.Tables.Add(dt);

                    dt.AcceptChanges();
                    ds.AcceptChanges();

                    //get the memory stream
                    MemoryStream ms = ExcelTool.ExportDataSet(ds);

                    useLog.Debug(String.Format("User {0} {1} Executed report {2} ", user.FirstName, user.LastName, reportInput.ReportName));

                    //unload all bulk values for current report
                    foreach (BulkTemplate template in reportInput.BulkTemplates) {
                        template.UnloadValues(user, sessionId);
                    }//end method

                    //export the document
                    ContentDisposition cd = new System.Net.Mime.ContentDisposition() {
                        FileName = ds.DataSetName + ".xlsx",
                        Inline = false
                    };
                    Response.AppendHeader("Content-Disposition", cd.ToString());

                    return File(ms.ToArray(), MimeMapping.GetMimeMapping("export.xlsx"), "export.xlsx");
                }
                useLog.Debug(String.Format("User {0} {1} recieved report {2} ", user.FirstName, user.LastName, report.ReportName));
            }
            return View(ctReport);
        }//end method

        /// <summary>
        /// Determine whether user has access to report. 
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public bool HasAccess(Report report) {
            bool hasAccess = false;
            //check that user has access to reports
            User usr = null;
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();
                usr = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
            }

            hasAccess = usr.HasAccess(report);

            foreach (var grp in usr.Groups)
                hasAccess = hasAccess || grp.HasAccess(report);

            return hasAccess;
        }//end method

        /// <summary>
        /// Upload excel file for bulk upload. 
        /// </summary>
        /// <param name="excelFile"></param>
        /// <param name="jsonBulkTemplate"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExcelUpload(HttpPostedFileBase excelFile, String jsonBulkTemplate, int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            BulkTemplate template = JsonConvert.DeserializeObject<BulkTemplate>(jsonBulkTemplate);

            try {
                if (excelFile != null && excelFile.ContentLength > 0) {
                    using (var reader = new System.IO.BinaryReader(excelFile.InputStream)) {
                        byte[] content = reader.ReadBytes(excelFile.ContentLength);

                        MemoryStream ms = new MemoryStream(content);

                        DataSet ds = ExcelTool.ImportToDataSet(ms);

                        //get user object via email address. 
                        User user = null;
                        using (UserRepository usrRepo = new UserRepository()) {
                            usrRepo.BeginTransaction();
                            user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
                        }

                        template.LoadValues(ds, user, HttpContext.Session.SessionID);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex) {
                log.Error("Error loading values from excel sheet. ", ex);
                throw;
            }

            //gui needs to know which report is selected on postback
            ViewBag.SelectedReport = reportId;
            //gui needs current report page posted back
            ViewBag.ReportCurrentPage = GetCurrentReportPage();

            //re-submit report to view
            Report report = null;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    report = repo.GetById(reportId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting report by id. ", ex);
                throw;
            }

            //will be a problem getting back to the selection we originally wanted
            return View(report);
        }//end method

        #region ReportRelatedPaging
        public int ReportPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"] = value; }
        }//end method

        public int ReportPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"] = value; }
        }//end method

        /// <summary>
        /// Get an unsearched page of reports. 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetReportUnsearchedPage(int pageNumber, int pageSize) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get this user
            User user = null;
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();
                user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
            }//end using

            List<Report> reports = null;
            String json = null;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    //reports = repo.GetReportsPaged(pageNumber, pageSize).ToList();
                    reports = repo.GetUsersReportsPaged(user, pageNumber, pageSize).ToList();
                }
                
                //serialize Users to json
                JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                json = JsonConvert.SerializeObject(reports.ToArray(), settings);
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }

            return Content(json, "application/json");
        }//end method

        /// <summary>
        /// Get a searched page of reports. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetReportSearchedPage(String searchStr, int pageNumber, int pageSize) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get this user
            User user = null;
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();
                user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
            }//end using

            List<Report> reports = null;
            String json = null;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    reports = repo.SearchUsersReportsPaged(user, searchStr, pageNumber, pageSize).ToList();
                }

                //serialize Users to json
                JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                json = JsonConvert.SerializeObject(reports.ToArray(), settings);
            }
            catch (Exception ex) {
                log.Error("Error searched page of reports. ", ex);
                throw;
            }

            return Content(json, "application/json");
        }//end method

        /// <summary>
        /// Get the number of reports. 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetReportUnsearchedCount() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get this user
            User user = null;
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();
                user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
            }//end using

            int numReports = 0;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    numReports = repo.GetNumUsersReports(user);
                }
            }
            catch (Exception) {
                throw;
            }

            //serialize Users to json
            String json = String.Format("{{\"numReports\":{0}}}", numReports);

            return Content(json, "application/json");
        }//end method

        /// <summary>
        /// Get the number of reports in a search set. 
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public ActionResult GetReportSearchCount(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get this user
            User user = null;
            using (UserRepository usrRepo = new UserRepository()) {
                usrRepo.BeginTransaction();
                user = usrRepo.GetByEmail(Session["USER_NAME"].ToString());
            }//end using

            int numReports = 0;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    repo.BeginTransaction();
                    numReports = repo.GetTotalNumUsersSearchResults(user, searchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }

            //serialize Users to json
            String json = String.Format("{{\"numReports\":{0}}}", numReports);

            return Content(json, "application/json");
        }//end method
        #endregion

    }//end class

}//end namespace