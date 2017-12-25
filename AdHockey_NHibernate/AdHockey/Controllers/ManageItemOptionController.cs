using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageItemOptionController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public Option ctOption {
            get { return (Option)System.Web.HttpContext.Current.Session["CT_OPTION"]; }
            set { System.Web.HttpContext.Current.Session["CT_OPTION"] = value; }
        }//end property

        public int TemplateId {
            get { return (int)System.Web.HttpContext.Current.Session["User_ID"]; }
            set { System.Web.HttpContext.Current.Session["User_ID"] = value; }
        }//end property

        // GET: ManageOption
        public ActionResult Index(int UserId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsOptionSearch = false;
            OptionPageNumber = 1;
            OptionPageSize = 2;

            UserId = UserId;

            ctOptionPage = GetCurrentOptionPage();
            ViewBag.OptionPage = ctOptionPage;

            return View(ctOption);
            //return View();
        }//end method

        public List<Option> GetCurrentOptionPage() {
            List<Option> list = null;
            using (OptionRepository repo = new OptionRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetOptionsPaged(OptionPageNumber, OptionPageSize);
                }
                catch (Exception ex) {
                    log.Error("Error page of Option's. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitOption(Option option) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    using (OptionRepository optRepo = new OptionRepository()) {
                        optRepo.BeginTransaction();

                        Template template = null;
                        using (TemplateItemRepository tmpRepo = new TemplateItemRepository()) {
                            tmpRepo.BeginTransaction();
                            template = tmpRepo.GetById(TemplateId);
                        }

                        option.TemplateId = TemplateId;
                        option.Template = template;
                        optRepo.Insert(option);
                    }
                }
                catch (Exception ex) {
                    log.Error("Error Submitting option. ", ex);
                    throw;
                }
                ModelState.Clear();

                ctOptionPage = GetCurrentOptionPage();
                ViewBag.OptionPage = ctOptionPage;

                ctOption = option;

                return View("Index", ctOption);
            }
            else {
                ctOptionPage = GetCurrentOptionPage();
                ViewBag.OptionPage = ctOptionPage;

                ctOption = option;
                return View("Index", ctOption);
            }

        }//end method

        public ActionResult DeleteOption(int OptionId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Option Option = null;

            //delete Option
            using (OptionRepository repo = new OptionRepository()) {
                try {
                    repo.BeginTransaction();
                    Option = repo.GetById(OptionId);
                    repo.Delete(OptionId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting option. ", ex);
                    throw;
                }
            }

            ctOption = Option;
            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            OptionSetDataSource();
            ViewBag.OptionPage = ctOptionPage;
            ViewBag.OptionSearchStr = OptionSearchStr;

            return View("Index", ctOption);
        }//end method
        #endregion

        #region OptionRelatedPaging
        public List<AdHockey.Models.Option> ctOptionPage {
            get { return (List<Option>)System.Web.HttpContext.Current.Session["CT_OPTION_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_OPTION_PAGE"] = value; }
        }//end method

        public bool IsOptionSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_OPTION_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_OPTION_SEARCH"] = value; }
        }//end method

        public String OptionSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["OPTION_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["OPTION_SEARCH_STRING"] = value; }
        }//end method

        public int OptionPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["OPTION_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["OPTION_PAGE_NUMBER"] = value; }
        }//end method

        public int OptionPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["OPTION_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["OPTION_PAGE_SIZE"] = value; }
        }//end method

        public List<Option> GetOptionUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (OptionRepository repo = new OptionRepository()) {
                    return repo.GetOptionsPaged(OptionPageNumber, OptionPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of options. ", ex);
                throw;
            }
        }//end method

        public List<Option> GetOptionSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (OptionRepository repo = new OptionRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchOptionsPaged(OptionSearchStr, OptionPageNumber, OptionPageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of options. ", ex);
                throw;
            }
        }//end method

        public int GetOptionUnserarchedCount() {
            try {
                using (OptionRepository repo = new OptionRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumOptions();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of options. ", ex);
                throw;
            }
        }//end method

        public int GetOptionSearchCount(String searchStr) {
            try {
                using (OptionRepository repo = new OptionRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(OptionSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void OptionSetDataSource() {
            try {
                if (IsOptionSearch) {
                    ctOptionPage = GetOptionSearchedPage(OptionSearchStr, OptionPageNumber, OptionPageSize);
                }
                else {
                    ctOptionPage = GetOptionUnsearchedPage(OptionPageNumber, OptionPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctOptionPage = new List<Option>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of options. ", ex);
                throw;
            }
        }//end method

        public ActionResult OptionFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            OptionPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult OptionPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (OptionPageNumber != 1) {
                --OptionPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult OptionNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numOptionItems = -1;

            if (IsOptionSearch)
                numOptionItems = GetOptionSearchCount(OptionSearchStr);
            else
                numOptionItems = GetOptionUnserarchedCount();

            if (Math.Ceiling((decimal)numOptionItems / (decimal)OptionPageSize) >= OptionPageNumber + 1) {
                ++OptionPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult OptionLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numOptions = -1;

            if (IsOptionSearch)
                numOptions = GetOptionSearchCount(OptionSearchStr);
            else
                numOptions = GetOptionUnserarchedCount();

            OptionPageNumber = (numOptions / OptionPageSize) + ((numOptions / OptionPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsOptionSearch = true;
            OptionSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsOptionSearch = false;
            OptionSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace