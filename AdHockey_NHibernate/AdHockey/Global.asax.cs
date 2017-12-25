using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

using AdHockey.Models;

namespace AdHockey
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //create database tables initialize database
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(Report).Assembly);
            new SchemaExport(cfg).Execute(true, true, false);

            InitializeDb initializer = new InitializeDb();

            //Schedules the cleanup of bulk template tables 
            JobScheduler.Start();
        }
    }
}
