using System;
using System.Data;
using Oracle.DataAccess.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AdHockey.Models {

    /// <summary>
    /// User is used to get generic variable parameters for inserting into report sql. 
    /// </summary>
    public class TemplateItem : Template {
        public TemplateItem() { }

        /// <summary>
        /// The report id this template is associated with. 
        /// </summary>
        public int ReportId {
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
        /// The name of the control you are using, for example datepicker or textbox
        /// </summary>
        public String ControlName {
            get; set;
        }

        /// <summary>
        /// The string value returned from the gui, the contents of the text box/control. 
        /// </summary>
        public virtual String StrVal {
            get; set;
        }

        private String alias;

        public virtual String Alias {
            get {
                return alias;
            }
            set {
                if (value.StartsWith(":"))
                    this.alias = value;
                else
                    this.alias = ":" + value;
            }
        }

        /// <summary>
        /// The drop down list options assuming the User requires them. 
        /// </summary>
        public virtual IList<Group> Options {
            get; set;
        }

        /// <summary>
        /// Creates an oracle parameter for usage in the Report object upohn execution of the report. 
        /// </summary>
        /// <returns></returns>
        public virtual OracleParameter CreateOracleParameter() {
            if (ClrType == typeof(Int32).ToString())
                return new OracleParameter(Alias, OracleDbType.Int32) { Direction = ParameterDirection.Input, Value = GetValue() };
            else if (ClrType == typeof(Int64).ToString())
                return new OracleParameter(Alias, OracleDbType.Int64) { Direction = ParameterDirection.Input, Value = GetValue() };
            else if (ClrType == typeof(decimal).ToString())
                return new OracleParameter(Alias, OracleDbType.Decimal) { Direction = ParameterDirection.Input, Value = GetValue() };
            else if (ClrType == typeof(float).ToString())
                return new OracleParameter(Alias, OracleDbType.Decimal) { Direction = ParameterDirection.Input, Value = GetValue() };
            else if (ClrType == typeof(char).ToString())
                return new OracleParameter(Alias, OracleDbType.Char) { Direction = ParameterDirection.Input, Value = GetValue() };
            else if (ClrType == typeof(String).ToString())
                return new OracleParameter(Alias, OracleDbType.Varchar2) { Direction = ParameterDirection.Input, Value = GetValue() };
            else if (ClrType == typeof(DateTime).ToString())
                return new OracleParameter(Alias, OracleDbType.Date) { Direction = ParameterDirection.Input, Value = GetValue() };
            else
                throw new NotImplementedException("The parameter type has not been implemented yet. ");
        }//end method

        /// <summary>
        /// Converts the value of this TemplateItem into an actual .net object for usage in the CreateOracleParameter() method. 
        /// </summary>
        /// <returns></returns>
        public virtual object GetValue() {
            return Convert.ChangeType(StrVal, Type.GetType(ClrType));
        }
    }//end class

}//end namespace