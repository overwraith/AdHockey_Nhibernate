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
    public class AddUserReportController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public User ctUser {
            get {
                return (User)System.Web.HttpContext.Current.Session["CT_REPORT"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_REPORT"] = value;
            }
        }//end method

        public int UserId {
            get {
                return (int)System.Web.HttpContext.Current.Session["REPORT_ID"];
            }
            set {
                System.Web.HttpContext.Current.Session["REPORT_ID"] = value;
            }
        }//end method

        public User GetUser(int userId) {
            User group = null;
            using (UserRepository repo = new UserRepository()) {
                group = repo.GetById(userId);
            }
            return group;
        }//end method

        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ReportPageNumber = 1;
            ReportPageSize = 5;

            UserPageNumber = 1;
            UserPageSize = 5;

            RptUsrPageNumber = 1;
            RptUsrPageSize = 5;

            UserPageNumber = 1;
            UserPageSize = 5;

            IsReportSearch = false;
            IsRptUsrSearch = false;
            IsUserSearch = false;

            ViewBag.UserSearchStr = UserSearchStr;
            ViewBag.UserPage = GetUserUnsearchedPage(UserPageNumber, UserPageSize);

            return View();
        }//end method

        public ActionResult AddReport(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Report group = null;

            try {
                using (ReportRepository repo = new ReportRepository()) {
                    repo.BeginTransaction();
                    group = repo.GetById(groupId);
                }
            }
            catch (Exception ex) {
                log.Error("Error could not get report. ", ex);
                throw;
            }

            if (ctUser.Reports == null)
                ctUser.Reports = new List<Report>();

            //don't let browser add same person twice
            foreach (var grp in ctUser.Reports) {
                if (grp.ReportId == groupId)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }//end loop

            //add user to User
            ctUser.Reports.Add(group);

            try {
                using (UserRepository repo = new UserRepository()) {
                    repo.BeginTransaction();
                    repo.Update(ctUser);
                    repo.CommitTransaction();
                }
            }
            catch (Exception ex) {
                log.Error("Error could not update user. ", ex);
                throw;
            }

            return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        public ActionResult RemoveReport(int reportId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get the current group
            ctUser = GetUser(ctUser.UserId);

            if (ctUser.Reports == null)
                ctUser.Reports = new List<Report>();

            //remove user from list
            Report group = ctUser.Reports
                .Where(grp => grp.ReportId == reportId)
                .First();

            ctUser.Reports.Remove(group);

            //update the User
            try {
                using (UserRepository repo = new UserRepository()) {
                    repo.BeginTransaction();
                    repo.Update(ctUser);
                    repo.CommitTransaction();
                }
            }
            catch (Exception ex) {
                log.Error("Error could not update user. ", ex);
                throw;
            }

            return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSourceA() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            SetReportDataSource();

            try {
                if (IsUserSearch)
                    ctUserPage = GetUserSearchedPage(UserSearchStr, UserPageNumber, UserPageSize);
                else
                    ctUserPage = GetUserUnsearchedPage(UserPageNumber, UserPageSize);
            }
            catch (ArgumentException ex) {
                //is a blank page
                ctUserPage = new List<User>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }

            ViewBag.UserPage = ctUserPage;
            ViewBag.UserSearchStr = UserSearchStr;

            return View("Index", ctUser);
        }//end method

        public ActionResult SetDataSourceB(int userId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //store report id
            UserId = userId;

            //set the current group
            ctUser = GetUser(userId);
            SetUserDataSource(userId);

            SetReportDataSource();

            GrpUsrSetDataSource(userId);

            ViewBag.CheckedUserId = userId;

            try {
                using (UserRepository grpRepo = new UserRepository()) {
                    grpRepo.BeginTransaction();
                    ctUser = grpRepo.GetById(userId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting current user. ", ex);
                throw;
            }

            try {
                if (IsUserSearch)
                    ctUserPage = GetUserSearchedPage(UserSearchStr, UserPageNumber, UserPageSize);
                else
                    ctUserPage = GetUserUnsearchedPage(UserPageNumber, UserPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctUserPage = new List<User>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }

            ViewBag.UserPage = ctUserPage;
            ViewBag.UserSearchStr = UserSearchStr;

            return View("Index", ctUser);
        }//end method
        #endregion

        public void SetReportDataSource() {
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
        }//end method

        public void GrpUsrSetDataSource(int reportId) {
            try {
                if (IsRptUsrSearch)
                    ctRptUsrPage = GetRptUsrSearchedPage(RptUsrSearchStr, RptUsrPageNumber, RptUsrPageSize);
                else
                    ctRptUsrPage = GetRptUsrUnsearchedPage(RptUsrPageNumber, RptUsrPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctRptUsrPage = new List<Report>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }

            ViewBag.UserSearchStr = RptUsrSearchStr;
            ViewBag.UserReportPage = ctRptUsrPage;
        }//end method

        [Route("{reportId}")]
        public void SetUserDataSource(int reportId) {
            try {
                if (IsUserSearch)
                    ctUserPage = GetUserSearchedPage(UserSearchStr, UserPageNumber, UserPageSize);
                else
                    ctUserPage = GetUserUnsearchedPage(UserPageNumber, UserPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctUserPage = new List<User>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }

            ViewBag.UserPage = ctUserPage;
            ViewBag.UserSearchStr = UserSearchStr;
        }//end method

        //**********************************************************************
        //                      Available Users. 
        //**********************************************************************

        #region UserRelatedPaging
        public List<AdHockey.Models.User> ctUserPage {
            get { return (List<User>)System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"] = value; }
        }//end method

        public bool IsUserSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"] = value; }
        }//end method

        public String UserSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"] = value; }
        }//end method

        public int UserPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"] = value; }
        }//end method

        public int UserPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"] = value; }
        }//end method

        public List<User> GetUserUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetUsersPaged(UserPageNumber, UserPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }
        }//end method

        public List<User> GetUserSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.SearchUsersPaged(UserSearchStr, UserPageNumber, UserPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of users. ", ex);
                throw;
            }
        }//end method

        public int GetUserUnserarchedCount() {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetNumUsers();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting user count. ", ex);
                throw;
            }
        }//end method

        public int GetUserSearchCount(String searchStr) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetTotalNumSearchResults(UserSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of search results. ", ex);
                throw;
            }
        }//end method

        public ActionResult UserFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            UserPageNumber = 1;

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            catch (NullReferenceException ex) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult UserPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (UserPageNumber != 1) {
                --UserPageNumber;
                try {
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            return null;
        }//end method

        public ActionResult UserNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numTemplateItems = -1;

            if (IsUserSearch)
                numTemplateItems = GetUserSearchCount(UserSearchStr);
            else
                numTemplateItems = GetUserUnserarchedCount();

            if (Math.Ceiling((decimal)numTemplateItems / (decimal)UserPageSize) >= UserPageNumber + 1) {
                ++UserPageNumber;
                try {
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            return null;
        }//end method

        public ActionResult UserLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numUsers = -1;

            if (IsUserSearch)
                numUsers = GetUserSearchCount(UserSearchStr);
            else
                numUsers = GetUserUnserarchedCount();

            UserPageNumber = (numUsers / UserPageSize) + ((numUsers / UserPageSize) % 2 == 0 ? 0 : 1);

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
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

            IsUserSearch = true;
            UserSearchStr = searchStr;

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
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

            IsUserSearch = false;
            UserSearchStr = "";

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method
        #endregion


        //**********************************************************************
        //                      Available Reports. 
        //**********************************************************************

        #region ReportRelatedPaging
        public List<AdHockey.Models.Report> ctReportPage {
            get { return (List<Report>)System.Web.HttpContext.Current.Session["CT_GROUP_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_GROUP_PAGE"] = value; }
        }//end method

        public bool IsReportSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_GROUP_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_GROUP_SEARCH"] = value; }
        }//end method

        public String ReportSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["GROUP_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_SEARCH_STRING"] = value; }
        }//end method

        public int ReportPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_PAGE_NUMBER"] = value; }
        }//end method

        public int ReportPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_PAGE_SIZE"] = value; }
        }//end method

        public List<Report> GetReportUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetReportsPaged(ReportPageNumber, ReportPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting group page. ", ex);
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
                log.Error("Error getting searched report page. ", ex);
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
                log.Error("Error getting number of reports. ", ex);
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
                log.Error("Error getting group total number of search results. ", ex);
                throw;
            }
        }//end method

        public ActionResult ReportFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ReportPageNumber = 1;

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            catch (NullReferenceException) {
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
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException) {
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
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (UserId != null)
                        return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
                }
                catch (NullReferenceException) {
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
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult ReportExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsReportSearch = true;
            ReportSearchStr = searchStr;

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult ReportClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsReportSearch = false;
            ReportSearchStr = "";

            try {
                if (UserId != null)
                    return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method
        #endregion

        #region RptUsrRelatedPaging
        public List<AdHockey.Models.Report> ctRptUsrPage {
            get { return (List<Report>)System.Web.HttpContext.Current.Session["CT_USR_GRP_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_USR_GRP_PAGE"] = value; }
        }//end method

        public bool IsRptUsrSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_USR_GRP_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_USR_GRP_SEARCH"] = value; }
        }//end method

        public String RptUsrSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["USR_GRP_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_SEARCH_STRING"] = value; }
        }//end method

        public int RptUsrPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["USR_GRP_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_PAGE_NUMBER"] = value; }
        }//end method

        public int RptUsrPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["USR_GRP_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_PAGE_SIZE"] = value; }
        }//end method

        public List<Report> GetRptUsrUnsearchedPage(int pageNumber, int pageSize) {
            List<AdHockey.Models.Report> groups = null;
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    groups = repo.GetUserReportsPaged(UserId, RptUsrPageNumber, RptUsrPageSize).ToList();
                    return groups;
                }
            }
            catch (Exception ex) {
                log.Error("Error getting user's reports paged. ", ex);
                throw;
            }
        }//end method

        public List<Report> GetRptUsrSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.SearchUserReportsPaged(RptUsrSearchStr, UserId, RptUsrPageNumber, RptUsrPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error searching for user's reports paged. ", ex);
                throw;
            }
        }//end method

        public int GetRptUsrUnserarchedCount() {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetUserNumReports(UserId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting user's number of reports. ", ex);
                throw;
            }
        }//end method

        public int GetRptUsrSearchCount(String searchStr) {
            try {
                using (ReportRepository repo = new ReportRepository()) {
                    return repo.GetTotalNumUserReportSearchResults(RptUsrSearchStr, UserId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of user's reports. ", ex);
                throw;
            }
        }//end method

        public ActionResult RptUsrFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RptUsrPageNumber = 1;

            return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        public ActionResult RptUsrPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RptUsrPageNumber != 1) {
                --RptUsrPageNumber;
                return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            else
                return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        public ActionResult RptUsrNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRptUsrItems = -1;

            if (IsRptUsrSearch)
                numRptUsrItems = GetRptUsrSearchCount(RptUsrSearchStr);
            else
                numRptUsrItems = GetRptUsrUnserarchedCount();

            if (Math.Ceiling((decimal)numRptUsrItems / (decimal)RptUsrPageSize) >= RptUsrPageNumber + 1) {
                ++RptUsrPageNumber;
                return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
            }
            else
                return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        public ActionResult RptUsrLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRptUsrs = -1;

            if (IsRptUsrSearch)
                numRptUsrs = GetRptUsrSearchCount(RptUsrSearchStr);
            else
                numRptUsrs = GetRptUsrUnserarchedCount();

            RptUsrPageNumber = (numRptUsrs / RptUsrPageSize) + ((numRptUsrs / RptUsrPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        public ActionResult RptUsrExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRptUsrSearch = true;
            RptUsrSearchStr = searchStr;

            return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method

        public ActionResult RptUsrClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRptUsrSearch = false;
            RptUsrSearchStr = "";

            return RedirectToAction("SetDataSourceB", new { userId = ctUser.UserId });
        }//end method
        #endregion

    }//end class

}//end namespace