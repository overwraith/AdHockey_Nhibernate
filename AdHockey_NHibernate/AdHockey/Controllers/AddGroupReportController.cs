/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Repositories;
using AdHockey.Models;
using log4net;

namespace AdHockey.Controllers {
    public class AddGroupReportController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public Report ctReport {
            get {
                return (Report)System.Web.HttpContext.Current.Session["CT_REPORT"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_REPORT"] = value;
            }
        }//end method

        public int ReportId {
            get {
                return (int)System.Web.HttpContext.Current.Session["REPORT_ID"];
            }
            set {
                System.Web.HttpContext.Current.Session["REPORT_ID"] = value;
            }
        }//end method

        public Report GetReport(int reportId) {
            Report group = null;
            using (ReportRepository repo = new ReportRepository()) {
                group = repo.GetById(reportId);
            }
            return group;
        }//end method

        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            GroupPageNumber = 1;
            GroupPageSize = 5;

            ReportPageNumber = 1;
            ReportPageSize = 5;

            GrpRptPageNumber = 1;
            GrpRptPageSize = 5;

            ReportPageNumber = 1;
            ReportPageSize = 5;

            IsGroupSearch = false;
            IsGrpRptSearch = false;
            IsReportSearch = false;

            ViewBag.ReportSearchStr = ReportSearchStr;
            ViewBag.ReportPage = GetReportUnsearchedPage(ReportPageNumber, ReportPageSize);

            return View();
        }//end method

        public ActionResult AddGroup(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Group group = null;

            try {
                using (GroupRepository repo = new GroupRepository()) {
                    repo.BeginTransaction();
                    group = repo.GetById(groupId);
                }
            }
            catch (Exception ex) {
                log.Error("Error could not get group. ", ex);
                throw;
            }

            if (ctReport.Groups == null)
                ctReport.Groups = new List<Group>();

            //don't let browser add same person twice
            foreach (var grp in ctReport.Groups) {
                if (grp.GroupId == groupId)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }//end loop

            //add user to Report
            ctReport.Groups.Add(group);

            try {
                using (ReportRepository repo = new ReportRepository()) {
                    repo.BeginTransaction();
                    repo.Update(ctReport);
                    repo.CommitTransaction();
                }
            }
            catch (Exception ex) {
                log.Error("Error could not update report. ", ex);
                throw;
            }

            return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        public ActionResult RemoveGroup(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get the current group
            ctReport = GetReport(ctReport.ReportId);

            if (ctReport.Groups == null)
                ctReport.Groups = new List<Group>();

            //remove user from list
            Group group = ctReport.Groups
                .Where(grp => grp.GroupId == groupId)
                .First();

            ctReport.Groups.Remove(group);

            //update the Report
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    repo.BeginTransaction();
                    repo.Update(ctReport);
                    repo.CommitTransaction();
                }
            }
            catch (Exception ex) {
                log.Error("Error could not update report. ", ex);
                throw;
            }

            return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSourceA() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            SetGroupDataSource();

            try {
                if (IsReportSearch)
                    ctReportPage = GetReportSearchedPage(ReportSearchStr, ReportPageNumber, ReportPageSize);
                else
                    ctReportPage = GetReportUnsearchedPage(ReportPageNumber, ReportPageSize);
            }
            catch (ArgumentException ex) {
                //is a blank page
                ctReportPage = new List<Report>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }

            ViewBag.ReportPage = ctReportPage;
            ViewBag.ReportSearchStr = ReportSearchStr;

            return View("Index", ctReport);
        }//end method

        public ActionResult SetDataSourceB(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //store report id
            ReportId = reportId;

            //set the current group
            ctReport = GetReport(reportId);
            SetReportDataSource(reportId);

            SetGroupDataSource();

            GrpUsrSetDataSource(reportId);

            ViewBag.CheckedReportId = reportId;

            try {
                using (ReportRepository grpRepo = new ReportRepository()) {
                    grpRepo.BeginTransaction();
                    ctReport = grpRepo.GetById(reportId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting current report. ", ex);
                throw;
            }

            try {
                if (IsReportSearch)
                    ctReportPage = GetReportSearchedPage(ReportSearchStr, ReportPageNumber, ReportPageSize);
                else
                    ctReportPage = GetReportUnsearchedPage(ReportPageNumber, ReportPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctReportPage = new List<Report>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }

            ViewBag.ReportPage = ctReportPage;
            ViewBag.ReportSearchStr = ReportSearchStr;

            return View("Index", ctReport);
        }//end method
        #endregion

        public void SetGroupDataSource() {
            try {
                if (IsGroupSearch)
                    ctGroupPage = GetGroupSearchedPage(GroupSearchStr, GroupPageNumber, GroupPageSize);
                else
                    ctGroupPage = GetGroupUnsearchedPage(GroupPageNumber, GroupPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctGroupPage = new List<Group>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }

            ViewBag.GroupPage = ctGroupPage;
            ViewBag.GroupSearchStr = GroupSearchStr;
        }//end method

        public void GrpUsrSetDataSource(int reportId) {
            try {
                if (IsGrpRptSearch)
                    ctGrpRptPage = GetGrpRptSearchedPage(GrpRptSearchStr, GrpRptPageNumber, GrpRptPageSize);
                else
                    ctGrpRptPage = GetGrpRptUnsearchedPage(GrpRptPageNumber, GrpRptPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctGrpRptPage = new List<Group>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }

            ViewBag.ReportSearchStr = GrpRptSearchStr;
            ViewBag.ReportGroupPage = ctGrpRptPage;
        }//end method

        [Route("{reportId}")]
        public void SetReportDataSource(int reportId) {
            try {
                if (IsReportSearch)
                    ctReportPage = GetReportSearchedPage(ReportSearchStr, ReportPageNumber, ReportPageSize);
                else
                    ctReportPage = GetReportUnsearchedPage(ReportPageNumber, ReportPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctReportPage = new List<Report>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }

            ViewBag.ReportPage = ctReportPage;
            ViewBag.ReportSearchStr = ReportSearchStr;
        }//end method

        //**********************************************************************
        //                      Available Reports. 
        //**********************************************************************

        #region ReportRelatedPaging
        public List<AdHockey.Models.Report> ctReportPage {
            get { return (List<Report>)System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"] = value; }
        }//end method

        public bool IsReportSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"] = value; }
        }//end method

        public String ReportSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"] = value; }
        }//end method

        public int ReportPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"] = value; }
        }//end method

        public int ReportPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"] = value; }
        }//end method

        public List<Report> GetReportUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetReportsPaged(ReportPageNumber, ReportPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }
        }//end method

        public List<Report> GetReportSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.SearchReportsPaged(ReportSearchStr, ReportPageNumber, ReportPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of reports. ", ex);
                throw;
            }
        }//end method

        public int GetReportUnserarchedCount() {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetNumReports();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting report count. ", ex);
                throw;
            }
        }//end method

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

        public ActionResult ReportFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ReportPageNumber = 1;

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException ex) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult ReportPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (ReportPageNumber != 1) {
                --ReportPageNumber;
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            return null;
        }//end method

        public ActionResult ReportNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numReportItems = -1;

            if (IsReportSearch)
                numReportItems = GetReportSearchCount(ReportSearchStr);
            else
                numReportItems = GetReportUnserarchedCount();

            if (Math.Ceiling((decimal)numReportItems / (decimal)ReportPageSize) >= ReportPageNumber + 1) {
                ++ReportPageNumber;
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            return null;
        }//end method

        public ActionResult ReportLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numReports = -1;

            if (IsReportSearch)
                numReports = GetReportSearchCount(ReportSearchStr);
            else
                numReports = GetReportUnserarchedCount();

            ReportPageNumber = (numReports / ReportPageSize) + ((numReports / ReportPageSize) % 2 == 0 ? 0 : 1);

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException ex) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsReportSearch = true;
            ReportSearchStr = searchStr;

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException ex) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsReportSearch = false;
            ReportSearchStr = "";

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method
        #endregion


        //**********************************************************************
        //                      Available Groups. 
        //**********************************************************************

        #region GroupRelatedPaging
        public List<AdHockey.Models.Group> ctGroupPage {
            get { return (List<Group>)System.Web.HttpContext.Current.Session["CT_GROUP_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_GROUP_PAGE"] = value; }
        }//end method

        public bool IsGroupSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_GROUP_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_GROUP_SEARCH"] = value; }
        }//end method

        public String GroupSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["GROUP_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_SEARCH_STRING"] = value; }
        }//end method

        public int GroupPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_PAGE_NUMBER"] = value; }
        }//end method

        public int GroupPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_PAGE_SIZE"] = value; }
        }//end method

        public List<Group> GetGroupUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.GetGroupsPaged(GroupPageNumber, GroupPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting group page. ", ex);
                throw;
            }
        }//end method

        public List<Group> GetGroupSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.SearchGroupsPaged(GroupSearchStr, GroupPageNumber, GroupPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting searched group page. ", ex);
                throw;
            }
        }//end method

        public int GetGroupUnserarchedCount() {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.GetNumGroups();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetGroupSearchCount(String searchStr) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.GetTotalNumSearchResults(GroupSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting group total number of search results. ", ex);
                throw;
            }
        }//end method

        public ActionResult GroupFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            GroupPageNumber = 1;

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult GroupPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (GroupPageNumber != 1) {
                --GroupPageNumber;
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            return null;
        }//end method

        public ActionResult GroupNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numGroupItems = -1;

            if (IsGroupSearch)
                numGroupItems = GetGroupSearchCount(GroupSearchStr);
            else
                numGroupItems = GetGroupUnserarchedCount();

            if (Math.Ceiling((decimal)numGroupItems / (decimal)GroupPageSize) >= GroupPageNumber + 1) {
                ++GroupPageNumber;
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (ReportId != null)
                        return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            return null;
        }//end method

        public ActionResult GroupLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numGroups = -1;

            if (IsGroupSearch)
                numGroups = GetGroupSearchCount(GroupSearchStr);
            else
                numGroups = GetGroupUnserarchedCount();

            GroupPageNumber = (numGroups / GroupPageSize) + ((numGroups / GroupPageSize) % 2 == 0 ? 0 : 1);

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult GroupExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGroupSearch = true;
            GroupSearchStr = searchStr;

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult GroupClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGroupSearch = false;
            GroupSearchStr = "";

            try {
                if (ReportId != null)
                    return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method
        #endregion

        #region GrpRptRelatedPaging
        public List<AdHockey.Models.Group> ctGrpRptPage {
            get { return (List<Group>)System.Web.HttpContext.Current.Session["CT_USR_GRP_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_USR_GRP_PAGE"] = value; }
        }//end method

        public bool IsGrpRptSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_USR_GRP_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_USR_GRP_SEARCH"] = value; }
        }//end method

        public String GrpRptSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["USR_GRP_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_SEARCH_STRING"] = value; }
        }//end method

        public int GrpRptPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["USR_GRP_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_PAGE_NUMBER"] = value; }
        }//end method

        public int GrpRptPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["USR_GRP_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_PAGE_SIZE"] = value; }
        }//end method

        public List<Group> GetGrpRptUnsearchedPage(int pageNumber, int pageSize) {
            List<AdHockey.Models.Group> groups = null;
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    groups = repo.GetReportGroupsPaged(ReportId, GrpRptPageNumber, GrpRptPageSize).ToList();
                    return groups;
                }
            }
            catch (Exception ex) {
                log.Error("Error getting group's groups paged. ", ex);
                throw;
            }
        }//end method

        public List<Group> GetGrpRptSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.SearchReportGroupsPaged(GrpRptSearchStr, ReportId, GrpRptPageNumber, GrpRptPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error searching for reports's groups paged. ", ex);
                throw;
            }
        }//end method

        public int GetGrpRptUnserarchedCount() {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.GetReportNumGroups(ReportId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting report's number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetGrpRptSearchCount(String searchStr) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.GetTotalNumReportGroupSearchResults(GrpRptSearchStr, ReportId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of report's groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult GrpRptFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            GrpRptPageNumber = 1;

            return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        public ActionResult GrpRptPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (GrpRptPageNumber != 1) {
                --GrpRptPageNumber;
                return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            else
                return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        public ActionResult GrpRptNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numGrpRptItems = -1;

            if (IsGrpRptSearch)
                numGrpRptItems = GetGrpRptSearchCount(GrpRptSearchStr);
            else
                numGrpRptItems = GetGrpRptUnserarchedCount();

            if (Math.Ceiling((decimal)numGrpRptItems / (decimal)GrpRptPageSize) >= GrpRptPageNumber + 1) {
                ++GrpRptPageNumber;
                return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
            }
            else
                return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        public ActionResult GrpRptLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numGrpRpts = -1;

            if (IsGrpRptSearch)
                numGrpRpts = GetGrpRptSearchCount(GrpRptSearchStr);
            else
                numGrpRpts = GetGrpRptUnserarchedCount();

            GrpRptPageNumber = (numGrpRpts / GrpRptPageSize) + ((numGrpRpts / GrpRptPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        public ActionResult GrpRptExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGrpRptSearch = true;
            GrpRptSearchStr = searchStr;

            return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method

        public ActionResult GrpRptClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGrpRptSearch = false;
            GrpRptSearchStr = "";

            return RedirectToAction("SetDataSourceB", new { reportId = ctReport.ReportId });
        }//end method
        #endregion

    }//end class

}//end namespace