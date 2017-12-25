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
    public class AddUserGroupController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public Group ctGroup {
            get {
                return (Group)System.Web.HttpContext.Current.Session["CT_GROUP"];
            }
            set {
                System.Web.HttpContext.Current.Session["CT_GROUP"] = value;
            }
        }//end method

        public int GroupId {
            get {
                return (int)System.Web.HttpContext.Current.Session["GROUP_ID"];
            }
            set {
                System.Web.HttpContext.Current.Session["GROUP_ID"] = value;
            }
        }//end method

        public Group GetGroup(int groupId) {
            Group group = null;
            using (GroupRepository repo = new GroupRepository()) {
                group = repo.GetById(groupId);
            }
            return group;
        }//end method

        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            UserPageNumber = 1;
            UserPageSize = 5;

            GroupPageNumber = 1;
            GroupPageSize = 5;

            UsrGrpPageNumber = 1;
            UsrGrpPageSize = 5;

            GroupPageNumber = 1;
            GroupPageSize = 5;

            IsUserSearch = false;
            IsUsrGrpSearch = false;
            IsGroupSearch = false;

            ViewBag.GroupSearchStr = GroupSearchStr;
            ViewBag.GroupPage = GetGroupUnsearchedPage(GroupPageNumber, GroupPageSize);

            return View();
        }//end method

        public ActionResult AddUser(int userId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            User user = null;

            try {
                using (UserRepository repo = new UserRepository()) {
                    repo.BeginTransaction();
                    user = repo.GetById(userId);
                }
            }
            catch (Exception ex) {
                log.Error("Error could not get user. ", ex);
                throw;
            }

            if (ctGroup.Users == null)
                ctGroup.Users = new List<User>();

            //don't let browser add same person twice
            foreach (var usr in ctGroup.Users) {
                if (usr.UserId == userId)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }//end loop

            //add user to Group
            ctGroup.Users.Add(user);

            try {
                using (GroupRepository repo = new GroupRepository()) {
                    repo.BeginTransaction();
                    repo.Update(ctGroup);
                    repo.CommitTransaction();
                }
            }
            catch (Exception ex) {
                log.Error("Error could not update group. ", ex);
                throw;
            }

            return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        public ActionResult RemoveUser(int userId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //get the current group
            ctGroup = GetGroup(ctGroup.GroupId);

            if (ctGroup.Users == null)
                ctGroup.Users = new List<User>();

            //remove user from list
            User user = ctGroup.Users
                .Where(usr => usr.UserId == userId)
                .First();

            ctGroup.Users.Remove(user);

            //update the Group
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    repo.BeginTransaction();
                    repo.Update(ctGroup);
                    repo.CommitTransaction();
                }
            }
            catch (Exception ex) {
                log.Error("Error could not update group. ", ex);
                throw;
            }

            return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSourceA() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            SetUserDataSource();

            try {
                if (IsGroupSearch)
                    ctGroupPage = GetGroupSearchedPage(GroupSearchStr, GroupPageNumber, GroupPageSize);
                else
                    ctGroupPage = GetGroupUnsearchedPage(GroupPageNumber, GroupPageSize);
            }
            catch (ArgumentException ex) {
                //is a blank page
                ctGroupPage = new List<Group>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }

            ViewBag.GroupPage = ctGroupPage;
            ViewBag.GroupSearchStr = GroupSearchStr;

            return View("Index", ctGroup);
        }//end method

        public ActionResult SetDataSourceB(int groupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //set the current group
            ctGroup = GetGroup(groupId);
            SetUserDataSource();

            //set group id
            GroupId = groupId;

            SetGroupDataSource(groupId);
            GrpUsrSetDataSource(groupId);

            ViewBag.CheckedUserId = groupId;

            try {
                using (GroupRepository grpRepo = new GroupRepository()) {
                    grpRepo.BeginTransaction();
                    ctGroup = grpRepo.GetById(groupId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting current group. ", ex);
                throw;
            }

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

            return View("Index", ctGroup);
        }//end method
        #endregion

        public void SetUserDataSource() {
            try {
                if (IsGroupSearch)
                    ctUserPage = GetUserSearchedPage(UserSearchStr, UserPageNumber, UserPageSize);
                else
                    ctUserPage = GetUserUnsearchedPage(UserPageNumber, UserPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctUserPage = new List<User>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }

            ViewBag.UserPage = ctUserPage;
            ViewBag.UserSearchStr = UserSearchStr;
        }//end method

        public void GrpUsrSetDataSource(int groupId) {
            try {
                if (IsGroupSearch)
                    ctUsrGrpPage = GetUsrGrpSearchedPage(GroupSearchStr, GroupPageNumber, GroupPageSize);
                else
                    ctUsrGrpPage = GetUsrGrpUnsearchedPage(GroupPageNumber, GroupPageSize);
            }
            catch (ArgumentException) {
                //is a blank page
                ctUsrGrpPage = new List<User>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }

            ViewBag.GroupSearchStr = UsrGrpSearchStr;
            ViewBag.GroupUserPage = ctUsrGrpPage;
        }//end method

        [Route("{groupId}")]
        public void SetGroupDataSource(int groupId) {
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

        //**********************************************************************
        //                      Available Groups. 
        //**********************************************************************

        #region GroupRelatedPaging
        public List<AdHockey.Models.Group> ctGroupPage {
            get { return (List<Group>)System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_REPORT_PAGE"] = value; }
        }//end method

        public bool IsGroupSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_REPORT_SEARCH"] = value; }
        }//end method

        public String GroupSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_SEARCH_STRING"] = value; }
        }//end method

        public int GroupPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_NUMBER"] = value; }
        }//end method

        public int GroupPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["REPORT_PAGE_SIZE"] = value; }
        }//end method

        public List<Group> GetGroupUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    return repo.GetGroupsPaged(GroupPageNumber, GroupPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
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
                log.Error("Error getting page of groups. ", ex);
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
                log.Error("Error getting group count. ", ex);
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
                log.Error("Error getting number of search results. ", ex);
                throw;
            }
        }//end method

        public ActionResult GroupFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            GroupPageNumber = 1;

            try {
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            catch (NullReferenceException ex) {
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
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException ex) {
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
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException ex) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException ex) {
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
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
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

            IsGroupSearch = true;
            GroupSearchStr = searchStr;

            try {
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
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

            IsGroupSearch = false;
            GroupSearchStr = "";

            try {
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method
        #endregion


        //**********************************************************************
        //                      Available Users. 
        //**********************************************************************

        #region UserRelatedPaging
        public List<AdHockey.Models.User> ctUserPage {
            get { return (List<User>)System.Web.HttpContext.Current.Session["CT_GROUP_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_GROUP_PAGE"] = value; }
        }//end method

        public bool IsUserSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_GROUP_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_GROUP_SEARCH"] = value; }
        }//end method

        public String UserSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["GROUP_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_SEARCH_STRING"] = value; }
        }//end method

        public int UserPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_PAGE_NUMBER"] = value; }
        }//end method

        public int UserPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["GROUP_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["GROUP_PAGE_SIZE"] = value; }
        }//end method

        public List<User> GetUserUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetUsersPaged(UserPageNumber, UserPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting user page. ", ex);
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
                log.Error("Error getting searched user page. ", ex);
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
                log.Error("Error getting number of users. ", ex);
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
                log.Error("Error getting user total number of search results. ", ex);
                throw;
            }
        }//end method

        public ActionResult UserFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            UserPageNumber = 1;

            try {
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            catch (NullReferenceException) {
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
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException) {
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
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException) {
                    return RedirectToAction("SetDataSourceA");
                }
            }
            else {
                try {
                    if (GroupId != null)
                        return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
                }
                catch (NullReferenceException) {
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
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult UserExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUserSearch = true;
            UserSearchStr = searchStr;

            try {
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method

        public ActionResult UserClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUserSearch = false;
            UserSearchStr = "";

            try {
                if (GroupId != null)
                    return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            catch (NullReferenceException) {
                return RedirectToAction("SetDataSourceA");
            }
            return null;
        }//end method
        #endregion

        #region UsrGrpRelatedPaging
        public List<AdHockey.Models.User> ctUsrGrpPage {
            get { return (List<User>)System.Web.HttpContext.Current.Session["CT_USR_GRP_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_USR_GRP_PAGE"] = value; }
        }//end method

        public bool IsUsrGrpSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_USR_GRP_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_USR_GRP_SEARCH"] = value; }
        }//end method

        public String UsrGrpSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["USR_GRP_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_SEARCH_STRING"] = value; }
        }//end method

        public int UsrGrpPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["USR_GRP_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_PAGE_NUMBER"] = value; }
        }//end method

        public int UsrGrpPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["USR_GRP_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["USR_GRP_PAGE_SIZE"] = value; }
        }//end method

        public List<User> GetUsrGrpUnsearchedPage(int pageNumber, int pageSize) {
            List<AdHockey.Models.User> users = null;
            try {
                using (UserRepository repo = new UserRepository()) {
                    users = repo.GetGroupUsersPaged(GroupId, UsrGrpPageNumber, UsrGrpPageSize).ToList();
                    return users;
                }
            }
            catch (Exception ex) {
                log.Error("Error getting group's users paged. ", ex);
                throw;
            }
        }//end method

        public List<User> GetUsrGrpSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.SearchGroupUsersPaged(UsrGrpSearchStr, GroupId, UsrGrpPageNumber, UsrGrpPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error searching for group's users paged. ", ex);
                throw;
            }
        }//end method

        public int GetUsrGrpUnserarchedCount() {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetGroupNumUsers(GroupId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting group's number of users. ", ex);
                throw;
            }
        }//end method

        public int GetUsrGrpSearchCount(String searchStr) {
            try {
                using (UserRepository repo = new UserRepository()) {
                    return repo.GetTotalNumGroupUserSearchResults(UsrGrpSearchStr, GroupId);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of group's users. ", ex);
                throw;
            }
        }//end method

        public ActionResult UsrGrpFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            UsrGrpPageNumber = 1;

            return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        public ActionResult UsrGrpPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (UsrGrpPageNumber != 1) {
                --UsrGrpPageNumber;
                return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            else
                return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        public ActionResult UsrGrpNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numUsrGrpItems = -1;

            if (IsUsrGrpSearch)
                numUsrGrpItems = GetUsrGrpSearchCount(UsrGrpSearchStr);
            else
                numUsrGrpItems = GetUsrGrpUnserarchedCount();

            if (Math.Ceiling((decimal)numUsrGrpItems / (decimal)UsrGrpPageSize) >= UsrGrpPageNumber + 1) {
                ++UsrGrpPageNumber;
                return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
            }
            else
                return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        public ActionResult UsrGrpLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numUsrGrps = -1;

            if (IsUsrGrpSearch)
                numUsrGrps = GetUsrGrpSearchCount(UsrGrpSearchStr);
            else
                numUsrGrps = GetUsrGrpUnserarchedCount();

            UsrGrpPageNumber = (numUsrGrps / UsrGrpPageSize) + ((numUsrGrps / UsrGrpPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        public ActionResult UsrGrpExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUsrGrpSearch = true;
            UsrGrpSearchStr = searchStr;

            return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method

        public ActionResult UsrGrpClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsUsrGrpSearch = false;
            UsrGrpSearchStr = "";

            return RedirectToAction("SetDataSourceB", new { groupId = ctGroup.GroupId });
        }//end method
        #endregion

    }//end class

}//end namespace