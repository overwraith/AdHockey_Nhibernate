using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageGroupController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public Group ctGroup {
            get { return (Group)System.Web.HttpContext.Current.Session["CT_GROUP"]; }
            set { System.Web.HttpContext.Current.Session["CT_GROUP"] = value; }
        }//end property

        // GET: ManageGroup
        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGroupSearch = false;
            GroupPageNumber = 1;
            GroupPageSize = 2;

            ctGroupPage = GetCurrentGroupPage();
            ViewBag.GroupPage = ctGroupPage;

            return View(ctGroup);
            //return View();
        }//end method

        public List<Group> GetCurrentGroupPage() {
            List<Group> list = null;
            using (GroupRepository repo = new GroupRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetGroupsPaged(GroupPageNumber, GroupPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of groups. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitGroup(Group Group) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (GroupRepository repo = new GroupRepository()) {
                        repo.BeginTransaction();
                        repo.Insert(Group);
                        repo.CommitTransaction();
                    }
                }
                catch (Exception ex) {
                    log.Error("Error inserting group into database. ", ex);
                    throw;
                }
                ModelState.Clear();

                ctGroupPage = GetCurrentGroupPage();
                ViewBag.GroupPage = ctGroupPage;

                return View("Index", ctGroup);
            }
            else {
                ctGroupPage = GetCurrentGroupPage();
                ViewBag.GroupPage = ctGroupPage;

                ctGroup = Group;
                return View("Index", ctGroup);
            }

        }//end method

        public ActionResult DeleteGroup(int GroupId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Group Group = null;

            //delete Group
            using (GroupRepository repo = new GroupRepository()) {
                try {
                    repo.BeginTransaction();
                    Group = repo.GetById(GroupId);
                    repo.Delete(GroupId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting group from database. ", ex);
                    throw;
                }
            }

            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            GroupSetDataSource();
            ViewBag.GroupPage = ctGroupPage;
            ViewBag.GroupSearchStr = GroupSearchStr;

            return View("Index", ctGroup);
        }//end method
        #endregion

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
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<Group> GetGroupSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchGroupsPaged(GroupSearchStr, GroupPageNumber, GroupPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of groups. ", ex);
                throw;
            }
        }//end method

        public int GetGroupUnserarchedCount() {
            try {
                using (GroupRepository repo = new GroupRepository()) {
                    repo.BeginTransaction();
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
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(GroupSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void GroupSetDataSource() {
            try {
                if (IsGroupSearch) {
                    ctGroupPage = GetGroupSearchedPage(GroupSearchStr, GroupPageNumber, GroupPageSize);
                }
                else {
                    ctGroupPage = GetGroupUnsearchedPage(GroupPageNumber, GroupPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctGroupPage = new List<Group>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method
        
        public ActionResult GroupFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            GroupPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult GroupPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (GroupPageNumber != 1) {
                --GroupPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
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
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
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

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGroupSearch = true;
            GroupSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsGroupSearch = false;
            GroupSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace