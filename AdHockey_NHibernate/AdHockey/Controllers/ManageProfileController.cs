using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdHockey.Models;
using AdHockey.Repositories;

namespace AdHockey.Controllers {
    public class ManageProfileController : Controller {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("RollingErrorAppender");

        public Profile ctProfile {
            get { return (Profile)System.Web.HttpContext.Current.Session["CT_PROFILE"]; }
            set { System.Web.HttpContext.Current.Session["CT_PROFILE"] = value; }
        }//end property

        // GET: ManageProfile
        public ActionResult Index() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsProfileSearch = false;
            ProfilePageNumber = 1;
            ProfilePageSize = 2;


            IsRestrictedSchemaSearch = false;
            RestrictedSchemaPageNumber = 1;
            RestrictedSchemaPageSize = 3;

            IsRestrictedTableSearch = false;
            RestrictedTablePageNumber = 1;
            RestrictedTablePageSize = 3;

            IsRestrictedFieldSearch = false;
            RestrictedFieldPageNumber = 1;
            RestrictedFieldPageSize = 3;


            IsAuthorizedSchemaSearch = false;
            AuthorizedSchemaPageNumber = 1;
            AuthorizedSchemaPageSize = 3;

            IsAuthorizedTableSearch = false;
            AuthorizedTablePageNumber = 1;
            AuthorizedTablePageSize = 3;

            IsAuthorizedFieldSearch = false;
            AuthorizedFieldPageNumber = 1;
            AuthorizedFieldPageSize = 3;


            ctProfilePage = GetCurrentProfilePage();
            ViewBag.ProfilePage = ctProfilePage;

            ViewBag.ProfileNamed = false;
            ViewBag.ProfileName = "";

            ctProfile = new Models.Profile();

            return View("index", ctProfile);
            //return View();
        }//end method

        public List<Profile> GetCurrentProfilePage() {
            List<Profile> list = null;
            using (ProfileRepository repo = new ProfileRepository()) {
                try {
                    repo.BeginTransaction();
                    list = repo.GetProfilesPaged(ProfilePageNumber, ProfilePageSize);
                }
                catch (Exception ex) {
                    log.Error("Error getting page of groups. ", ex);
                    throw;
                }
            }
            return list;
        }//end method

        public ActionResult SubmitProfile(Profile profile, string Command) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            if (ModelState.IsValid) {
                try {
                    if (Command == "SubmitName") {
                        ctProfile.ProfileName = profile.ProfileName;
                        ViewBag.ProfileNamed = true;
                        ViewBag.ProfileName = ctProfile.ProfileName;

                        InitSearchBoxes();
                    }
                    else if (Command == "SubmitProfile") {
                        ctProfile.ProfileName = profile.ProfileName;
                        using (ProfileRepository repo = new ProfileRepository()) {
                            repo.BeginTransaction();
                            repo.Insert(ctProfile);
                            repo.CommitTransaction();
                        }
                    }
                }
                catch (Exception ex) {
                    log.Error("Error inserting group into database. ", ex);
                    throw;
                }
                ModelState.Clear();

                ctProfilePage = GetCurrentProfilePage();
                ViewBag.ProfilePage = ctProfilePage;

                return View("Index", ctProfile);
            }
            else {
                ctProfilePage = GetCurrentProfilePage();
                ViewBag.ProfilePage = ctProfilePage;

                ctProfile.ProfileName = profile.ProfileName;
                return View("Index", ctProfile);
            }

        }//end method

        public void InitSearchBoxes() {
            if(ctProfile.AuthorizedFields != null)
                ViewBag.AuthFieldNames = ctProfile.AuthorizedFields.Select(field => field.ColumnName).Distinct().ToList();
            if (ctProfile.AuthorizedTables != null)
                ViewBag.AuthTableNames = ctProfile.AuthorizedTables.Select(table => table.TableName).Distinct().ToList();
            if (ctProfile.AuthorizedSchemas != null)
                ViewBag.AuthSchemaNames = ctProfile.AuthorizedSchemas.Select(schema => schema.SchemaName).Distinct().ToList();
            if (ctProfile.RestrictedFields != null)
                ViewBag.RestrFieldNames = ctProfile.RestrictedFields.Select(field => field.ColumnName).Distinct().ToList();
            if (ctProfile.RestrictedTables != null)
                ViewBag.RestrTableNames = ctProfile.RestrictedTables.Select(table => table.TableName).Distinct().ToList();
            if (ctProfile.RestrictedSchemas != null)
                ViewBag.RestrSchemaNames = ctProfile.RestrictedSchemas.Select(schema => schema.SchemaName).Distinct().ToList();
        }//end method

        public ActionResult DeleteProfile(int ProfileId) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile Profile = null;

            //delete Profile
            using (ProfileRepository repo = new ProfileRepository()) {
                try {
                    repo.BeginTransaction();
                    Profile = repo.GetById(ProfileId);
                    repo.Delete(ProfileId);
                }
                catch (Exception ex) {
                    log.Error("Error deleting group from database. ", ex);
                    throw;
                }
            }

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AddField(AuthorizedField field) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile profile = ctProfile;

            //check if list is unassigned
            if (profile.AuthorizedFields == null)
                profile.AuthorizedFields = new List<AuthorizedField>();

            //add field to profile
            profile.AuthorizedFields.Add(field);

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AddTable(AuthorizedTable table) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile profile = ctProfile;

            //check if list is unassigned
            if (profile.AuthorizedTables == null)
                profile.AuthorizedTables = new List<AuthorizedTable>();

            //add field to profile
            profile.AuthorizedTables.Add(table);

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AddSchema(AuthorizedSchema schema) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile profile = ctProfile;

            //check if list is unassigned
            if (profile.AuthorizedSchemas == null)
                profile.AuthorizedSchemas = new List<AuthorizedSchema>();

            //add field to profile
            profile.AuthorizedSchemas.Add(schema);

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method


        public ActionResult AddRestrictedField(RestrictedField column) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile profile = ctProfile;

            //check if list is unassigned
            if (profile.RestrictedFields == null)
                profile.RestrictedFields = new List<RestrictedField>();

            //add field to profile
            profile.RestrictedFields.Add(column);

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AddRestrictedTable(RestrictedTable table) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile profile = ctProfile;

            //check if list is unassigned
            if (profile.RestrictedTables == null)
                profile.RestrictedTables = new List<RestrictedTable>();

            //add field to profile
            profile.RestrictedTables.Add(table);

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AddRestrictedSchema(RestrictedSchema schema) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            Profile profile = ctProfile;

            //check if list is unassigned
            if (profile.RestrictedSchemas == null)
                profile.RestrictedSchemas = new List<RestrictedSchema>();

            //add field to profile
            profile.RestrictedSchemas.Add(schema);

            ViewBag.ProfileNamed = true;
            ViewBag.ProfileName = ctProfile.ProfileName;

            InitSearchBoxes();

            return RedirectToAction("SetDataSource");
        }//end method

        #region GlobalPagingRelated
        public ActionResult SetDataSource() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ProfileSetDataSource();
            ViewBag.ProfilePage = ctProfilePage;
            ViewBag.ProfileSearchStr = ProfileSearchStr;

            if (ctProfile.ProfileName != null) {
                ViewBag.ProfileNamed = true;
                ViewBag.ProfileName = ctProfile.ProfileName;
            }
            else {
                ViewBag.ProfileNamed = false;
                ViewBag.ProfileName = "";
            }

            AuthorizedSchemaSetDataSource();
            AuthorizedTableSetDataSource();
            AuthorizedFieldSetDataSource();
            RestrictedSchemaSetDataSource();
            RestrictedTableSetDataSource();
            RestrictedFieldSetDataSource();

            ViewBag.AuthFieldPage = ctAuthorizedFieldPage;
            ViewBag.AuthTablePage = ctAuthorizedTablePage;
            ViewBag.AuthSchemaPage = ctAuthorizedSchemaPage;
            ViewBag.RestrFieldPage = ctRestrictedFieldPage;
            ViewBag.RestrTablePage = ctRestrictedTablePage;
            ViewBag.RestrSchemaPage = ctRestrictedSchemaPage;

            InitSearchBoxes();

            return View("Index", ctProfile);
        }//end method
        #endregion

        #region ProfileRelatedPaging
        public List<AdHockey.Models.Profile> ctProfilePage {
            get { return (List<Profile>)System.Web.HttpContext.Current.Session["CT_PROFILE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_PROFILE_PAGE"] = value; }
        }//end method

        public bool IsProfileSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_PROFILE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_PROFILE_SEARCH"] = value; }
        }//end method

        public String ProfileSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["PROFILE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["PROFILE_SEARCH_STRING"] = value; }
        }//end method

        public int ProfilePageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["PROFILE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["PROFILE_PAGE_NUMBER"] = value; }
        }//end method

        public int ProfilePageSize {
            get { return (int)System.Web.HttpContext.Current.Session["PROFILE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["PROFILE_PAGE_SIZE"] = value; }
        }//end method

        public List<Profile> GetProfileUnsearchedPage(int pageNumber, int pageSize) {
            try {
                using (ProfileRepository repo = new ProfileRepository()) {
                    return repo.GetProfilesPaged(ProfilePageNumber, ProfilePageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<Profile> GetProfileSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                using (ProfileRepository repo = new ProfileRepository()) {
                    repo.BeginTransaction();
                    return repo.SearchProfilesPaged(ProfileSearchStr, ProfilePageNumber, ProfilePageSize).ToList();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of groups. ", ex);
                throw;
            }
        }//end method

        public int GetProfileUnserarchedCount() {
            try {
                using (ProfileRepository repo = new ProfileRepository()) {
                    repo.BeginTransaction();
                    return repo.GetNumProfiles();
                }
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetProfileSearchCount(String searchStr) {
            try {
                using (ProfileRepository repo = new ProfileRepository()) {
                    repo.BeginTransaction();
                    return repo.GetTotalNumSearchResults(ProfileSearchStr);
                }
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void ProfileSetDataSource() {
            try {
                if (IsProfileSearch) {
                    ctProfilePage = GetProfileSearchedPage(ProfileSearchStr, ProfilePageNumber, ProfilePageSize);
                }
                else {
                    ctProfilePage = GetProfileUnsearchedPage(ProfilePageNumber, ProfilePageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctProfilePage = new List<Profile>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult ProfileFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            ProfilePageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ProfilePreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (ProfilePageNumber != 1) {
                --ProfilePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ProfileNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numProfileItems = -1;

            if (IsProfileSearch)
                numProfileItems = GetProfileSearchCount(ProfileSearchStr);
            else
                numProfileItems = GetProfileUnserarchedCount();

            if (Math.Ceiling((decimal)numProfileItems / (decimal)ProfilePageSize) >= ProfilePageNumber + 1) {
                ++ProfilePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ProfileLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numProfiles = -1;

            if (IsProfileSearch)
                numProfiles = GetProfileSearchCount(ProfileSearchStr);
            else
                numProfiles = GetProfileUnserarchedCount();

            ProfilePageNumber = (numProfiles / ProfilePageSize) + ((numProfiles / ProfilePageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsProfileSearch = true;
            ProfileSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult ClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsProfileSearch = false;
            ProfileSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        #region AuthorizedSchemaRelatedPaging
        public List<AdHockey.Models.AuthorizedSchema> ctAuthorizedSchemaPage {
            get { return (List<AuthorizedSchema>)System.Web.HttpContext.Current.Session["CT_AUTH_SCHEMA_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTH_SCHEMA_PAGE"] = value; }
        }//end method

        public bool IsAuthorizedSchemaSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTH_SCHEMA_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTH_SCHEMA_SEARCH"] = value; }
        }//end method

        public String AuthorizedSchemaSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTH_SCHEMA_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_SCHEMA_SEARCH_STRING"] = value; }
        }//end method

        public int AuthorizedSchemaPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTH_SCHEMA_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_SCHEMA_PAGE_NUMBER"] = value; }
        }//end method

        public int AuthorizedSchemaPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTH_SCHEMA_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_SCHEMA_PAGE_SIZE"] = value; }
        }//end method

        public List<AuthorizedSchema> GetAuthorizedSchemaUnsearchedPage(int pageNumber, int pageSize) {
            try {
                    return ctProfile.AuthorizedSchemas
                        .Skip((AuthorizedSchemaPageNumber - 1) * AuthorizedSchemaPageSize)
                        .Take(AuthorizedSchemaPageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<AuthorizedSchema> GetAuthorizedSchemaSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.AuthorizedSchemas
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Skip((AuthorizedSchemaPageNumber - 1) * AuthorizedSchemaPageSize)
                        .Take(AuthorizedSchemaPageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Authorized Schemas. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedSchemaUnserarchedCount() {
            try {
                return ctProfile.AuthorizedSchemas.Count();
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedSchemaSearchCount(String searchStr) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.AuthorizedSchemas
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Count();
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void AuthorizedSchemaSetDataSource() {
            try {
                if (IsAuthorizedSchemaSearch) {
                    ctAuthorizedSchemaPage = GetAuthorizedSchemaSearchedPage(AuthorizedSchemaSearchStr, AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize);
                }
                else {
                    ctAuthorizedSchemaPage = GetAuthorizedSchemaUnsearchedPage(AuthorizedSchemaPageNumber, AuthorizedSchemaPageSize);
                }
            }
            catch (ArgumentException ex) {
                //is a blank page
                ctAuthorizedSchemaPage = new List<AuthorizedSchema>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult SchemaFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedSchemaPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SchemaPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (AuthorizedSchemaPageNumber != 1) {
                --AuthorizedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SchemaNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedSchemaItems = -1;

            if (IsAuthorizedSchemaSearch)
                numAuthorizedSchemaItems = GetAuthorizedSchemaSearchCount(AuthorizedSchemaSearchStr);
            else
                numAuthorizedSchemaItems = GetAuthorizedSchemaUnserarchedCount();

            if (Math.Ceiling((decimal)numAuthorizedSchemaItems / (decimal)AuthorizedSchemaPageSize) >= AuthorizedSchemaPageNumber + 1) {
                ++AuthorizedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SchemaLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedSchemas = -1;

            if (IsAuthorizedSchemaSearch)
                numAuthorizedSchemas = GetAuthorizedSchemaSearchCount(AuthorizedSchemaSearchStr);
            else
                numAuthorizedSchemas = GetAuthorizedSchemaUnserarchedCount();

            AuthorizedSchemaPageNumber = (numAuthorizedSchemas / AuthorizedSchemaPageSize) + ((numAuthorizedSchemas / AuthorizedSchemaPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SchemaExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedSchemaSearch = true;
            AuthorizedSchemaSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult SchemaClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedSchemaSearch = false;
            AuthorizedSchemaSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        #region AuthorizedTableRelatedPaging
        public List<AdHockey.Models.AuthorizedTable> ctAuthorizedTablePage {
            get { return (List<AuthorizedTable>)System.Web.HttpContext.Current.Session["CT_AUTH_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTH_TABLE_PAGE"] = value; }
        }//end method

        public bool IsAuthorizedTableSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTH_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTH_TABLE_SEARCH"] = value; }
        }//end method

        public String AuthorizedTableSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTH_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int AuthorizedTablePageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTH_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int AuthorizedTablePageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTH_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<AuthorizedTable> GetAuthorizedTableUnsearchedPage(int pageNumber, int pageSize) {
            try {
                return ctProfile.AuthorizedTables
                    .Skip((AuthorizedTablePageNumber - 1) * AuthorizedTablePageSize)
                    .Take(AuthorizedTablePageSize)
                    .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<AuthorizedTable> GetAuthorizedTableSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.AuthorizedTables
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Skip((AuthorizedTablePageNumber - 1) * AuthorizedTablePageSize)
                        .Take(AuthorizedTablePageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Authorized Schemas. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedTableUnserarchedCount() {
            try {
                return ctProfile.AuthorizedTables.Count();
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedTableSearchCount(String searchStr) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.AuthorizedTables
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Count();
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void AuthorizedTableSetDataSource() {
            try {
                if (IsAuthorizedTableSearch) {
                    ctAuthorizedTablePage = GetAuthorizedTableSearchedPage(AuthorizedTableSearchStr, AuthorizedTablePageNumber, AuthorizedTablePageSize);
                }
                else {
                    ctAuthorizedTablePage = GetAuthorizedTableUnsearchedPage(AuthorizedTablePageNumber, AuthorizedTablePageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctAuthorizedTablePage = new List<AuthorizedTable>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult TableFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedTablePageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult AuthorizedTablePreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (AuthorizedTablePageNumber != 1) {
                --AuthorizedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TableNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedTableItems = -1;

            if (IsAuthorizedTableSearch)
                numAuthorizedTableItems = GetAuthorizedTableSearchCount(AuthorizedTableSearchStr);
            else
                numAuthorizedTableItems = GetAuthorizedTableUnserarchedCount();

            if (Math.Ceiling((decimal)numAuthorizedTableItems / (decimal)AuthorizedTablePageSize) >= AuthorizedTablePageNumber + 1) {
                ++AuthorizedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TableLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedTables = -1;

            if (IsAuthorizedTableSearch)
                numAuthorizedTables = GetAuthorizedTableSearchCount(AuthorizedTableSearchStr);
            else
                numAuthorizedTables = GetAuthorizedTableUnserarchedCount();

            AuthorizedTablePageNumber = (numAuthorizedTables / AuthorizedTablePageSize) + ((numAuthorizedTables / AuthorizedTablePageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TableExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedTableSearch = true;
            AuthorizedTableSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult TableClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedTableSearch = false;
            AuthorizedTableSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        #region AuthorizedFieldsRelatedPaging
        public List<AdHockey.Models.AuthorizedField> ctAuthorizedFieldPage {
            get { return (List<AuthorizedField>)System.Web.HttpContext.Current.Session["CT_AUTH_FIELD_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_AUTH_FIELD_PAGE"] = value; }
        }//end method

        public bool IsAuthorizedFieldSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_AUTH_FIELD_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_AUTH_FIELD_SEARCH"] = value; }
        }//end method

        public String AuthorizedFieldSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["AUTH_FIELD_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_FIELD_SEARCH_STRING"] = value; }
        }//end method

        public int AuthorizedFieldPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["AUTH_FIELD_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_FIELD_PAGE_NUMBER"] = value; }
        }//end method

        public int AuthorizedFieldPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["AUTH_FIELD_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["AUTH_FIELD_PAGE_SIZE"] = value; }
        }//end method

        public List<AuthorizedField> GetAuthorizedFieldsUnsearchedPage(int pageNumber, int pageSize) {
            try {
                return ctProfile.AuthorizedFields
                    .Skip((AuthorizedFieldPageNumber - 1) * AuthorizedFieldPageSize)
                    .Take(AuthorizedFieldPageSize)
                    .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<AuthorizedField> GetAuthorizedFieldsSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.AuthorizedFields
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Skip((AuthorizedFieldPageNumber - 1) * AuthorizedFieldPageSize)
                        .Take(AuthorizedFieldPageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Authorized Schemas. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedFieldsUnserarchedCount() {
            try {
                return ctProfile.AuthorizedFields.Count();
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetAuthorizedFieldsSearchCount(String searchStr) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.AuthorizedFields
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Count();
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void AuthorizedFieldSetDataSource() {
            try {
                if (IsAuthorizedFieldSearch) {
                    ctAuthorizedFieldPage = GetAuthorizedFieldsSearchedPage(AuthorizedFieldSearchStr, AuthorizedFieldPageNumber, AuthorizedFieldPageSize);
                }
                else {
                    ctAuthorizedFieldPage = GetAuthorizedFieldsUnsearchedPage(AuthorizedFieldPageNumber, AuthorizedFieldPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctAuthorizedFieldPage = new List<AuthorizedField>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult FieldFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            AuthorizedFieldPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult FieldPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (AuthorizedFieldPageNumber != 1) {
                --AuthorizedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult FieldNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedFieldsItems = -1;

            if (IsAuthorizedFieldSearch)
                numAuthorizedFieldsItems = GetAuthorizedFieldsSearchCount(AuthorizedFieldSearchStr);
            else
                numAuthorizedFieldsItems = GetAuthorizedFieldsUnserarchedCount();

            if (Math.Ceiling((decimal)numAuthorizedFieldsItems / (decimal)AuthorizedFieldPageSize) >= AuthorizedFieldPageNumber + 1) {
                ++AuthorizedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult FieldLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numAuthorizedFieldss = -1;

            if (IsAuthorizedFieldSearch)
                numAuthorizedFieldss = GetAuthorizedFieldsSearchCount(AuthorizedFieldSearchStr);
            else
                numAuthorizedFieldss = GetAuthorizedFieldsUnserarchedCount();

            AuthorizedFieldPageNumber = (numAuthorizedFieldss / AuthorizedFieldPageSize) + ((numAuthorizedFieldss / AuthorizedFieldPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult FieldExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedFieldSearch = true;
            AuthorizedFieldSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult FieldsClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsAuthorizedFieldSearch = false;
            AuthorizedFieldSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        #region RestrictedSchemaRelatedPaging
        public List<AdHockey.Models.RestrictedSchema> ctRestrictedSchemaPage {
            get { return (List<RestrictedSchema>)System.Web.HttpContext.Current.Session["CT_RESTRICTED_SCHEMA_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_RESTRICTED_SCHEMA_PAGE"] = value; }
        }//end method

        public bool IsRestrictedSchemaSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_RESTRICTED_SCHEMA_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_RESTRICTED_SCHEMA_SEARCH"] = value; }
        }//end method

        public String RestrictedSchemaSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["RESTRICTED_SCHEMA_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_SCHEMA_SEARCH_STRING"] = value; }
        }//end method

        public int RestrictedSchemaPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["RESTRICTED_SCHEMA_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_SCHEMA_PAGE_NUMBER"] = value; }
        }//end method

        public int RestrictedSchemaPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["RESTRICTED_SCHEMA_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_SCHEMA_PAGE_SIZE"] = value; }
        }//end method

        public List<RestrictedSchema> GetRestrictedSchemaUnsearchedPage(int pageNumber, int pageSize) {
            try {
                return ctProfile.RestrictedSchemas
                    .Skip((RestrictedSchemaPageNumber - 1) * RestrictedSchemaPageSize)
                    .Take(RestrictedSchemaPageSize)
                    .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<RestrictedSchema> GetRestrictedSchemaSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.RestrictedSchemas
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Skip((RestrictedSchemaPageNumber - 1) * RestrictedSchemaPageSize)
                        .Take(RestrictedSchemaPageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Restricted Schemas. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedSchemaUnserarchedCount() {
            try {
                return ctProfile.RestrictedSchemas.Count();
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedSchemaSearchCount(String searchStr) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.RestrictedSchemas
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Count();
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void RestrictedSchemaSetDataSource() {
            try {
                if (IsRestrictedSchemaSearch) {
                    ctRestrictedSchemaPage = GetRestrictedSchemaSearchedPage(RestrictedSchemaSearchStr, RestrictedSchemaPageNumber, RestrictedSchemaPageSize);
                }
                else {
                    ctRestrictedSchemaPage = GetRestrictedSchemaUnsearchedPage(RestrictedSchemaPageNumber, RestrictedSchemaPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctRestrictedSchemaPage = new List<RestrictedSchema>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult RestrictedSchemaFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedSchemaPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RestrictedSchemaPageNumber != 1) {
                --RestrictedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedSchemaItems = -1;

            if (IsRestrictedSchemaSearch)
                numRestrictedSchemaItems = GetRestrictedSchemaSearchCount(RestrictedSchemaSearchStr);
            else
                numRestrictedSchemaItems = GetRestrictedSchemaUnserarchedCount();

            if (Math.Ceiling((decimal)numRestrictedSchemaItems / (decimal)RestrictedSchemaPageSize) >= RestrictedSchemaPageNumber + 1) {
                ++RestrictedSchemaPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedSchemas = -1;

            if (IsRestrictedSchemaSearch)
                numRestrictedSchemas = GetRestrictedSchemaSearchCount(RestrictedSchemaSearchStr);
            else
                numRestrictedSchemas = GetRestrictedSchemaUnserarchedCount();

            RestrictedSchemaPageNumber = (numRestrictedSchemas / RestrictedSchemaPageSize) + ((numRestrictedSchemas / RestrictedSchemaPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedSchemaSearch = true;
            RestrictedSchemaSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedSchemaClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedSchemaSearch = false;
            RestrictedSchemaSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        #region RestrictedTableRelatedPaging
        public List<AdHockey.Models.RestrictedTable> ctRestrictedTablePage {
            get { return (List<RestrictedTable>)System.Web.HttpContext.Current.Session["CT_RESTRICTED_TABLE_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_RESTRICTED_TABLE_PAGE"] = value; }
        }//end method

        public bool IsRestrictedTableSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_RESTRICTED_TABLE_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_RESTRICTED_TABLE_SEARCH"] = value; }
        }//end method

        public String RestrictedTableSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["RESTRICTED_TABLE_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_TABLE_SEARCH_STRING"] = value; }
        }//end method

        public int RestrictedTablePageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["RESTRICTED_TABLE_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_TABLE_PAGE_NUMBER"] = value; }
        }//end method

        public int RestrictedTablePageSize {
            get { return (int)System.Web.HttpContext.Current.Session["RESTRICTED_TABLE_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_TABLE_PAGE_SIZE"] = value; }
        }//end method

        public List<RestrictedTable> GetRestrictedTableUnsearchedPage(int pageNumber, int pageSize) {
            try {
                return ctProfile.RestrictedTables
                    .Skip((RestrictedTablePageNumber - 1) * RestrictedTablePageSize)
                    .Take(RestrictedTablePageSize)
                    .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<RestrictedTable> GetRestrictedTableSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.RestrictedTables
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Skip((RestrictedTablePageNumber - 1) * RestrictedTablePageSize)
                        .Take(RestrictedTablePageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Restricted Schemas. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedTableUnserarchedCount() {
            try {
                return ctProfile.RestrictedTables.Count();
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedTableSearchCount(String searchStr) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.RestrictedTables
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Count();
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void RestrictedTableSetDataSource() {
            try {
                if (IsRestrictedTableSearch) {
                    ctRestrictedTablePage = GetRestrictedTableSearchedPage(RestrictedTableSearchStr, RestrictedTablePageNumber, RestrictedTablePageSize);
                }
                else {
                    ctRestrictedTablePage = GetRestrictedTableUnsearchedPage(RestrictedTablePageNumber, RestrictedTablePageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctRestrictedTablePage = new List<RestrictedTable>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult RestrictedTableFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedTablePageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTablePreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RestrictedTablePageNumber != 1) {
                --RestrictedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTableNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedTableItems = -1;

            if (IsRestrictedTableSearch)
                numRestrictedTableItems = GetRestrictedTableSearchCount(RestrictedTableSearchStr);
            else
                numRestrictedTableItems = GetRestrictedTableUnserarchedCount();

            if (Math.Ceiling((decimal)numRestrictedTableItems / (decimal)RestrictedTablePageSize) >= RestrictedTablePageNumber + 1) {
                ++RestrictedTablePageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTableLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedTables = -1;

            if (IsRestrictedTableSearch)
                numRestrictedTables = GetRestrictedTableSearchCount(RestrictedTableSearchStr);
            else
                numRestrictedTables = GetRestrictedTableUnserarchedCount();

            RestrictedTablePageNumber = (numRestrictedTables / RestrictedTablePageSize) + ((numRestrictedTables / RestrictedTablePageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTableExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedTableSearch = true;
            RestrictedTableSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedTableClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedTableSearch = false;
            RestrictedTableSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

        #region RestrictedFieldsRelatedPaging
        public List<AdHockey.Models.RestrictedField> ctRestrictedFieldPage {
            get { return (List<RestrictedField>)System.Web.HttpContext.Current.Session["CT_RESTRICTED_FIELD_PAGE"]; }
            set { System.Web.HttpContext.Current.Session["CT_RESTRICTED_FIELD_PAGE"] = value; }
        }//end method

        public bool IsRestrictedFieldSearch {
            get { return (bool)System.Web.HttpContext.Current.Session["IS_RESTRICTED_FIELD_SEARCH"]; }
            set { System.Web.HttpContext.Current.Session["IS_RESTRICTED_FIELD_SEARCH"] = value; }
        }//end method

        public String RestrictedFieldSearchStr {
            get { return (string)System.Web.HttpContext.Current.Session["RESTRICTED_FIELD_SEARCH_STRING"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_FIELD_SEARCH_STRING"] = value; }
        }//end method

        public int RestrictedFieldPageNumber {
            get { return (int)System.Web.HttpContext.Current.Session["RESTRICTED_FIELD_PAGE_NUMBER"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_FIELD_PAGE_NUMBER"] = value; }
        }//end method

        public int RestrictedFieldPageSize {
            get { return (int)System.Web.HttpContext.Current.Session["RESTRICTED_FIELD_PAGE_SIZE"]; }
            set { System.Web.HttpContext.Current.Session["RESTRICTED_FIELD_PAGE_SIZE"] = value; }
        }//end method

        public List<RestrictedField> GetRestrictedFieldsUnsearchedPage(int pageNumber, int pageSize) {
            try {
                return ctProfile.RestrictedFields
                    .Skip((RestrictedFieldPageNumber - 1) * RestrictedFieldPageSize)
                    .Take(RestrictedFieldPageSize)
                    .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public List<RestrictedField> GetRestrictedFieldsSearchedPage(String searchStr, int pageNumber, int pageSize) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.RestrictedFields
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Skip((RestrictedFieldPageNumber - 1) * RestrictedFieldPageSize)
                        .Take(RestrictedFieldPageSize)
                        .ToList();
            }
            catch (Exception ex) {
                log.Error("Error getting searched page of Restricted Schemas. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedFieldsUnserarchedCount() {
            try {
                return ctProfile.RestrictedFields.Count();
            }
            catch (Exception ex) {
                log.Error("Error getting number of groups. ", ex);
                throw;
            }
        }//end method

        public int GetRestrictedFieldsSearchCount(String searchStr) {
            try {
                String[] toks = searchStr.ToLower().Split();
                return ctProfile.RestrictedFields
                        .Where(schema => toks.Any(str => str.Contains(schema.SchemaName)))
                        .Count();
            }
            catch (Exception ex) {
                log.Error("Error getting total number of search results. ", ex);
                throw;
            }
        }//end method

        public void RestrictedFieldSetDataSource() {
            try {
                if (IsRestrictedFieldSearch) {
                    ctRestrictedFieldPage = GetRestrictedFieldsSearchedPage(RestrictedFieldSearchStr, RestrictedFieldPageNumber, RestrictedFieldPageSize);
                }
                else {
                    ctRestrictedFieldPage = GetRestrictedFieldsUnsearchedPage(RestrictedFieldPageNumber, RestrictedFieldPageSize);
                }
            }
            catch (ArgumentException) {
                //is a blank page
                ctRestrictedFieldPage = new List<RestrictedField>();
            }
            catch (Exception ex) {
                log.Error("Error getting page of groups. ", ex);
                throw;
            }
        }//end method

        public ActionResult RestrictedFieldFirstPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            RestrictedFieldPageNumber = 1;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldPreviousPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            //determine whether we are on the first page
            if (RestrictedFieldPageNumber != 1) {
                --RestrictedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldNextPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedFieldsItems = -1;

            if (IsRestrictedFieldSearch)
                numRestrictedFieldsItems = GetRestrictedFieldsSearchCount(RestrictedFieldSearchStr);
            else
                numRestrictedFieldsItems = GetRestrictedFieldsUnserarchedCount();

            if (Math.Ceiling((decimal)numRestrictedFieldsItems / (decimal)RestrictedFieldPageSize) >= RestrictedFieldPageNumber + 1) {
                ++RestrictedFieldPageNumber;
                return RedirectToAction("SetDataSource");
            }
            else
                return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldLastPage() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            int numRestrictedFieldss = -1;

            if (IsRestrictedFieldSearch)
                numRestrictedFieldss = GetRestrictedFieldsSearchCount(RestrictedFieldSearchStr);
            else
                numRestrictedFieldss = GetRestrictedFieldsUnserarchedCount();

            RestrictedFieldPageNumber = (numRestrictedFieldss / RestrictedFieldPageSize) + ((numRestrictedFieldss / RestrictedFieldPageSize) % 2 == 0 ? 0 : 1);

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrcitedFieldExecuteSearch(String searchStr) {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedFieldSearch = true;
            RestrictedFieldSearchStr = searchStr;

            return RedirectToAction("SetDataSource");
        }//end method

        public ActionResult RestrictedFieldsClearSearch() {
            //check that user is logged in
            if (Session["USER_NAME"] == null)
                return RedirectToAction("index", "Login");

            IsRestrictedFieldSearch = false;
            RestrictedFieldSearchStr = "";

            return RedirectToAction("SetDataSource");
        }//end method
        #endregion

    }//end class

}//end namespace