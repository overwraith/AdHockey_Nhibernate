using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using ADOX;//Requires Microsoft ADO Ext. 2.8 for DDL and Security
using ADODB;

namespace AdHockey.Utilities {

    public static class AccessTool {

        public static bool ExportAccessDb(String fileName, DataSet ds) {
            bool result = false;

            ADOX.Catalog cat = new ADOX.Catalog();
            ADOX.Table table = new ADOX.Table();

            try {
                cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + fileName + "; Jet OLEDB:Engine Type=5");

                //create tables
                for (int i = 0; i < ds.Tables.Count; i++) {
                    var ctTable = ds.Tables[i];

                    //Create the table and it's fields. 
                    table.Name = ctTable.TableName;

                    CreateAccessCols(table, ctTable);

                    cat.Tables.Append(table);
                }//end method

                //now close the database
                ADODB.Connection conn = cat.ActiveConnection as ADODB.Connection;
                if (conn != null)
                    conn.Close();

                result = true;
            }
            catch (Exception ex) {
                result = false;
            }

            cat = null;

            FillAccessDb(fileName, ds);

            return result;
        }//end method

        private static void CreateAccessCols(Table table, DataTable ctTable) {

            foreach (DataColumn ctCol in ctTable.Columns) {
                if (ctCol.DataType == typeof(String))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adVarWChar);
                else if (ctCol.DataType == typeof(char))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adVarWChar);
                else if (ctCol.DataType == typeof(int))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adSmallInt);
                else if (ctCol.DataType == typeof(long))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adInteger);
                else if (ctCol.DataType == typeof(decimal))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adSingle);
                else if (ctCol.DataType == typeof(double))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adDouble);
                else if (ctCol.DataType == typeof(float))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adNumeric);
                else if (ctCol.DataType == typeof(DateTime))
                    table.Columns.Append(ctCol.ColumnName, ADOX.DataTypeEnum.adDate);
                else
                    throw new NotImplementedException("Access Column data type has not been implemented. ");
            }//end loop

        }//end method

        private static void FillAccessDb(String fileName, DataSet ds) {
            System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection();
            conn.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName;
            try {
                foreach (DataTable ctTable in ds.Tables) {
                    conn.Open();

                    String queryPart1 = null;
                    String queryPart2 = null;

                    foreach (DataRow dr in ctTable.Rows) {
                        String query = CreateInsertStatement(ctTable, ref queryPart1, ref queryPart2, dr);

                        OleDbCommand cmd = new OleDbCommand(query, conn);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) {
                throw;
            }
            finally {
                conn.Close();
            }
        }//end method

        private static String CreateInsertStatement(DataTable ctTable, ref String queryPart1, ref String queryPart2, DataRow dr) {
            queryPart1 = String.Format("INSERT INTO {0}", ctTable.TableName);
            queryPart2 = " VALUES (";

            //build insert statement
            for (int j = 0; j < ctTable.Columns.Count; j++) {
                var ctCol = ctTable.Columns[j];
                queryPart1 += ctCol.ColumnName + (j == ctTable.Columns.Count - 1 ? ")" : ", ");
                queryPart2 += "\'" + dr[j].ToString() + "\'" + (j == ctTable.Columns.Count - 1 ? ");" : ",");
            }//end loop

            String query = queryPart1 + queryPart2;
            return query;
        }//end method

    }//end class

}//end namespace