using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace AdHockey {
    /// <summary>
    /// Used to retry commands when they throw an exception, ex database commands. 
    /// </summary>
    public class RetryUtility {
        //global randoms are more random
        private static Random rand = new Random();

        /// <summary>
        /// Retrys a function with a randomized exponential backoff. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="sleepInterval"></param>
        /// <returns></returns>
        public static T Retry<T>(Func<T> func, int sleepInterval) {
            const int NUM_TIMES = 3;
            //loop 3 times
            for (int i = 0; i < NUM_TIMES; i++) {
                try {
                    return func.Invoke();
                }
                catch (Exception ex) {
                    if (i == NUM_TIMES)
                        throw ex;
                }

                //exponential backoff
                Thread.Sleep(sleepInterval *= rand.Next(2, 4));
            }//end loop
            return default(T);
        }//end method
    }//end class
}//end namespace