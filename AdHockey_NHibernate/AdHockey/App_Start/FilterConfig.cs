using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace AdHockey {

    public class FilterConfig {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());

            //require https for entire application
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["IsProd"])) {
                filters.Add(new RequireHttpsAttribute());
            }
        }//end method

    }//end class

}//end namespace
