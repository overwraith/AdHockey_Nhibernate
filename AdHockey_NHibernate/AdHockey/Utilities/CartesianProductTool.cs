/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdHockey.Utilities {

    /// <summary>
    /// Cartesian product tool. 
    /// </summary>
    public class CartesianProductTool {
        /// <summary>
        /// Gets the cartesian product of several inputs. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static List<List<T>> CartesianProduct<T>(List<List<T>> lists) {
            List<List<T>> resultLists = new List<List<T>>();
            if (lists.Count() == 0) {
                resultLists.Add(new List<T>());
                return resultLists;
            }
            else {
                List<T> firstList = lists.ElementAt(0);
                List<List<T>> remainingLists = CartesianProduct(lists.GetRange(1, lists.Count() - 1));
                foreach (T condition in firstList) {
                    foreach (List<T> remainingList in remainingLists) {
                        List<T> resultList = new List<T>();
                        resultList.Add(condition);
                        resultList.AddRange(remainingList);
                        resultLists.Add(resultList);
                    }
                }
            }
            return resultLists;
        }//end method
    }//end class

}//end namespace