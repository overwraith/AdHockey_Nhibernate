/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

//using Oracle.ManagedDataAccess.Client;
using Oracle.DataAccess.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace AdHockey.Models {

    /// <summary>
    /// Upload multiple values into a table to preform processing. Turn on bulk User logic from Report object. 
    /// Requires a supporting table structure and the AdHockey package. 
    /// </summary>
    public class BulkTemplate : Template {

        /// <summary>
        /// The name of the values column to be pulled out of the database. 
        /// </summary>
        public virtual String ValueDescriptor {
            get; set;
        }

        private String clrType;

        /// <summary>
        /// The type of value to return from the GUI textbox/Control. 
        /// </summary>
        public virtual String ClrType {
            get {
                return clrType;
            }
            set {
                if (Type.GetType(value) != null)
                    clrType = value;
                else
                    throw new ArgumentException("Not provided a valid .net type. ");
            }
        }

        /// <summary>
        /// The drop down list options assuming the User requires them. 
        /// </summary>
        public virtual IList<Group> Options {
            get; set;
        }

        public BulkTemplate() {
            ValueDescriptor = "VALUES";
        }

        /// <summary>
        /// Gets the connection string used to connect to the database. 
        /// </summary>
        /// <returns></returns>
        private String GetConnectionString() {
            String connStr = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
            return connStr;
        }

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        public virtual void LoadValues(DataSet data, User user, String sessionId) {
            if (ClrType == "System.Char")
                LoadValuesChar(data.Tables[0], user, sessionId);
            else if (ClrType == "System.Int16")
                LoadValuesInt16(data.Tables[0], user, sessionId);
            else if (ClrType == "System.Int32")
                LoadValuesInt32(data.Tables[0], user, sessionId);
            else if (ClrType == "System.Int64")
                LoadValuesInt64(data.Tables[0], user, sessionId);
            else if (ClrType == "System.Decimal")
                LoadValuesDecimal(data.Tables[0], user, sessionId);
            else if (ClrType == "System.Float")
                LoadValuesFloat(data.Tables[0], user, sessionId);
            else if (ClrType == "System.DateTime")
                LoadValuesDateTime(data.Tables[0], user, sessionId);
            if (ClrType == "System.String")
                LoadValuesString(data.Tables[0], user, sessionId);
            else
                throw new NotImplementedException("That data type is not implemented yet. ");
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesInt16(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            Int16[] values = data.AsEnumerable()
                .Select(row => row.Field<Int16>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_INT16";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Int16) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesInt32(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            Int32[] values = data.AsEnumerable()
                .Select(row => row.Field<Int32>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_INT32";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Int32) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesInt64(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            Int64[] values = data.AsEnumerable()
                .Select(row => row.Field<Int64>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_INT64";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Int64) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesChar(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            char[] values = data.AsEnumerable()
                .Select(row => row.Field<char>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_CHAR";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Char) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesDecimal(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            decimal[] values = data.AsEnumerable()
                .Select(row => row.Field<decimal>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_DECIMAL";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Decimal) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesFloat(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            float[] values = data.AsEnumerable()
                .Select(row => row.Field<float>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_FLOAT";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Decimal) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesDateTime(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            DateTime[] values = data.AsEnumerable()
                .Select(row => row.Field<DateTime>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_DATETIME";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Date) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Loads values from excel document into bulk User tables. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void LoadValuesString(DataTable data, User user, String sessionId) {
            OracleConnection conn = null;
            string[] values = data.AsEnumerable()
                .Select(row => row.Field<string>(ValueDescriptor))
                .ToArray();

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.BULK_UPLOAD_STRING";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter("I_ARR", OracleDbType.Varchar2) {
                        Value = values,
                        Direction = ParameterDirection.Input,
                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                        Size = values.Length
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch(Exception ex) { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>s
        public void UnloadValues(User user, String sessionId) {
            if (ClrType == "System.String")
                UnloadValuesInt16(user, sessionId);
            else if (ClrType == "System.Char")
                UnloadValuesChar(user, sessionId);
            else if (ClrType == "System.Int16")
                UnloadValuesInt16(user, sessionId);
            else if (ClrType == "System.Int32")
                UnloadValuesInt32(user, sessionId);
            else if (ClrType == "System.Int64")
                UnloadValuesInt64(user, sessionId);
            else if (ClrType == "System.Decimal")
                UnloadValuesDecimal(user, sessionId);
            else if (ClrType == "System.Float")
                UnloadValuesFloat(user, sessionId);
            else if (ClrType == "System.DateTime")
                UnloadValuesDateTime(user, sessionId);
            else
                throw new NotImplementedException("That data type is not implemented yet. ");
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesInt16(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_INT16";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesInt32(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_INT32";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesInt64(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_INT64";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesChar(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_CHAR";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesDecimal(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_DECIMAL";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesFloat(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_FLOAT";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        /// <summary>
        /// Unloads the values associated with this bulk User item from the database. 
        /// </summary>
        /// <param name="user"></param>
        private void UnloadValuesDateTime(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_DATETIME";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

        private void UnloadValuesString(User user, String sessionId) {
            OracleConnection conn = null;

            try {
                conn = new OracleConnection(GetConnectionString());

                conn.Open();
                using (OracleCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "ADHOCKEY.DELETE_BULK_ITEMS_STRING";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_TEMPLATE_ID",
                        Value = this.TemplateId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_USER_ID",
                        Value = user.UserId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.Parameters.Add(new OracleParameter() {
                        ParameterName = "I_SESSION_ID",
                        Value = sessionId,
                        Direction = ParameterDirection.Input,
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch { throw; }
            finally {
                conn.Close();
            }
        }//end method

    }//end class

}//end namespace