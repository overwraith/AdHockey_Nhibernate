/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace AdHockey.Models {
    /// <summary>
    /// Fields to restrict based on group. A black list. 
    /// </summary>
    public class RestrictedField {
        /// <summary>
        /// Unique identifier for restricted fields
        /// </summary>
        public virtual int FieldId {
            get; set;
        }

        /// <summary>
        /// Foreign key 
        /// </summary>
        public virtual int GroupId {
            get; set;
        }

        /// <summary>
        /// The group this restriction is associated with. 
        /// </summary>
        public virtual Group Group {
            get; set;
        }

        /// <summary>
        /// Database schema name. 
        /// </summary>
        public virtual String SchemaName {
            get; set;
        }

        /// <summary>
        /// Database table name. 
        /// </summary>
        public virtual String TableName {
            get; set;
        }

        /// <summary>
        /// Database column name. 
        /// </summary>
        public virtual String ColumnName {
            get; set;
        }

        /// <summary>
        /// Full Description of the field in question. 
        /// </summary>
        public virtual String Description {
            get; set;
        }

        /// <summary>
        /// Determine whether sql is authorized to run. 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool IsAuthorized(String sql) {
            //TODO: Could implement more robust logic for determining whether contains these perhaps with regexes
            //TODO CONT: Is possible will reject tables with like names. 

            //Debug.WriteLine("Contains A: " + (sql.Contains(String.Format("{0}.{1}.{2}", SchemaName, TableName, ColumnName))));
            //Debug.WriteLine("Contains B: " + (sql.Contains(String.Format("{0}.{1}", TableName, ColumnName))));
            //Debug.WriteLine("Contains C: " + (sql.Contains(String.Format("{0}", ColumnName))));

            //Debug.WriteLine("And A: " + (sql.Contains(String.Format("{0}.{1}.{2}", SchemaName, TableName, ColumnName))
            //        || sql.Contains(String.Format("{0}.{1}", TableName, ColumnName))));
            //Debug.WriteLine("And B: " + (sql.Contains(String.Format("{0}.{1}.{2}", SchemaName, TableName, ColumnName))
            //        || sql.Contains(String.Format("{0}.{1}", TableName, ColumnName))
            //        || sql.Contains(String.Format("{0}", ColumnName))));

            return !(sql.Contains(String.Format("{0}.{1}.{2}", SchemaName, TableName, ColumnName))
                    || sql.Contains(String.Format("{0}.{1}", TableName, ColumnName))
                    || sql.Contains(String.Format("{0}", ColumnName)));
        }//end method
    }//end class

}//end namespace