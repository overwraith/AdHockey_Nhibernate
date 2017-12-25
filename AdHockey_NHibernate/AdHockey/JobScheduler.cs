/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.DataAccess.Client;
using System.Configuration;
using System.Data;

using Quartz;
using Quartz.Impl;
using AdHockey;

namespace AdHockey {

    /// <summary>
    /// Schedules all periodic background tasks. 
    /// </summary>
    public class JobScheduler {
        /// <summary>
        /// Execute table cleanup at 12:00 every friday. 
        /// </summary>
        public static void Start() {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<TableCleanupJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                     .OnDaysOfTheWeek(new DayOfWeek[] { DayOfWeek.Friday })
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23, 59))
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);

        }//end method

    }//end class

    /// <summary>
    /// Truncate all bulk load tables. 
    /// </summary>
    public class TableCleanupJob : IJob {

        public TableCleanupJob() {

        }

        /// <summary>
        /// Execute the truncation of all bulk load tables. 
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IJobExecutionContext context) {
            //execute truncate commands on all bulk load tables
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_INT32"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_INT64"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_INT16"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_CHAR"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_DECIMAL"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_FLOAT"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_DATETIME"), 1000);
            RetryUtility.Retry<int>(() => ExecuteSql("TRUNCATE TABLE BULK_ITEMS_STRING"), 1000);
        }//end method

        /// <summary>
        /// Gets the database connection string. 
        /// </summary>
        /// <returns></returns>
        private String GetConnectionString() {
            String connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
            return connStr;
        }//end method

        /// <summary>
        /// Execute sql against the database. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteSql(String sql) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    return cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

    }//end class

}//end namespace