/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

//Add reference to WindowsBase .NET library
using System.IO.Packaging;
using System.Globalization;

namespace AdHockey.Utilities {
    public class ExcelTool {

        public static DataSet ImportToDataSet(MemoryStream ms) {
            DataSet ds = new DataSet();

            using (var spreadsheetDocument = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(ms, false)) {
                var workbookPart = spreadsheetDocument.WorkbookPart;
                var sheets = from sheet in workbookPart.Workbook
                             .Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>()
                             select sheet;
                var numSheets = sheets.Count();
                for (int i = 0; i < numSheets; i++) {
                    var worksheetPart = workbookPart.WorksheetParts.ElementAt(i);
                    var reader = DocumentFormat.OpenXml.OpenXmlReader.Create(worksheetPart);
                    //create new data table for every sheet
                    var dt = new DataTable();
                    bool firstRow = true;
                    //add name to data table
                    dt.TableName = sheets.ElementAt(i).Name;
                    //add data to data table
                    while (reader.Read()) {
                        List<String> rowContent = new List<String>();
                        if (reader.ElementType == typeof(DocumentFormat.OpenXml.Spreadsheet.Row)) {
                            reader.ReadFirstChild();
                            int columnIndex = 0;
                            do {
                                if (reader.ElementType == typeof(DocumentFormat.OpenXml.Spreadsheet.Cell)) {
                                    var cell = (DocumentFormat.OpenXml.Spreadsheet.Cell)reader.LoadCurrentElement();
                                    String cellValue;

                                    int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
                                    //take care of blank data
                                    if (columnIndex < cellColumnIndex) {
                                        do {
                                            rowContent[columnIndex] = null;
                                            columnIndex++;
                                        }
                                        while (columnIndex < cellColumnIndex);
                                    }

                                    if (cell.DataType != null) {
                                        if (cell.DataType == CellValues.SharedString) {
                                            var ssi = workbookPart.SharedStringTablePart
                                                .SharedStringTable.Elements<DocumentFormat.OpenXml.Spreadsheet
                                                .SharedStringItem>().ElementAt(int.Parse(cell.CellValue.InnerText));
                                            cellValue = ssi.Text.Text;
                                        }
                                        else
                                            cellValue = cell.CellValue.InnerText;
                                    }
                                    else
                                        cellValue = cell.CellValue.InnerText;
                                    rowContent.Add(cellValue);
                                }
                            } while (reader.ReadNextSibling());
                            if (firstRow) {
                                foreach (var heading in rowContent)
                                    dt.Columns.Add(heading, typeof(String));
                                firstRow = false;
                            }
                            else {
                                dt.Rows.Add(rowContent.ToArray());
                                rowContent.Clear();
                            }
                        }
                    }//end loop
                    //add a new row to the data set
                    ds.Tables.Add(dt);
                    dt.AcceptChanges();
                    ds.AcceptChanges();
                }//end loop
            }
            return ds;
        }//end method

        public static DataSet ImportToDataSet(MemoryStream ms, Type[][] sheetTypes) {
            DataSet ds = new DataSet();
            using (var spreadsheetDocument = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(ms, false)) {
                var workbookPart = spreadsheetDocument.WorkbookPart;
                var sheets = from sheet in workbookPart.Workbook
                             .Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>()
                             select sheet;
                var numSheets = sheets.Count();
                for (int i = 0; i < numSheets; i++) {
                    var worksheetPart = workbookPart.WorksheetParts.ElementAt(i);
                    var reader = DocumentFormat.OpenXml.OpenXmlReader.Create(worksheetPart);
                    //create a new data table for every sheet
                    var dt = new DataTable();
                    bool firstRow = true;
                    Type[] colTypes = sheetTypes[i];
                    //add name to data table
                    dt.TableName = sheets.ElementAt(i).Name;
                    while (reader.Read()) {
                        List<object> rowContent = new List<object>();
                        int colNum = 0;
                        if (reader.ElementType == typeof(DocumentFormat.OpenXml.Spreadsheet.Cell)) {
                            reader.ReadFirstChild();
                            int columnIndex = 0;
                            do {
                                if (reader.ElementType == typeof(DocumentFormat.OpenXml.Spreadsheet.Cell)) {
                                    var cell = (DocumentFormat.OpenXml.Spreadsheet.Cell)reader.LoadCurrentElement();

                                    int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
                                    //take care of blank data
                                    if (columnIndex < cellColumnIndex) {
                                        do {
                                            rowContent[columnIndex] = GetDefault(colTypes[columnIndex]);
                                            columnIndex++;
                                        }
                                        while (columnIndex < cellColumnIndex);
                                    }

                                    string cellValue = null;
                                    if (cell.DataType != null) {
                                        if (cell.DataType == CellValues.SharedString) {
                                            var ssi = workbookPart.SharedStringTablePart.SharedStringTable
                                                .Elements<DocumentFormat.OpenXml.Spreadsheet.SharedStringItem>()
                                                .ElementAt(int.Parse(cell.CellValue.InnerText));
                                            cellValue = ssi.Text.Text;
                                        }
                                        else {
                                            cellValue = cell.CellValue.InnerText;
                                        }
                                    }
                                    else {
                                        cellValue = cell.CellValue == null ? "" : cell.CellValue.InnerText;
                                    }
                                    if (firstRow)
                                        rowContent.Add(cellValue);
                                    else
                                        rowContent.Add(Convert.ChangeType(cellValue, colTypes[colNum]));
                                    colNum++;
                                }
                            } while (reader.ReadNextSibling());

                            if (firstRow) {
                                for (int j = 0; j < rowContent.Count(); j++) {
                                    dt.Columns.Add(rowContent.ElementAt(j).ToString(), colTypes[j]);
                                    firstRow = false;
                                }
                            }
                            else {
                                dt.Rows.Add(rowContent.ToArray());
                                rowContent.Clear();
                            }
                        }
                    }//end loop
                    //add data table to the data set
                    ds.Tables.Add(dt);
                    dt.AcceptChanges();
                    ds.AcceptChanges();
                }//end loop
            }
            return ds;
        }//end method

        private static object GetDefault(Type type) {
            if (type.IsValueType) {
                return Activator.CreateInstance(type);
            }
            return null;
        }//end method

        public static MemoryStream ExportDataSet(DataSet ds) {
            MemoryStream ms = new MemoryStream();
            using (var workbook = SpreadsheetDocument.Create(ms, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook)) {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();
                foreach (System.Data.DataTable table in ds.Tables) {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);
                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);
                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 1) {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }
                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() {
                        Id = relationshipId,
                        SheetId = sheetId,
                        Name = table.TableName
                    };
                    sheets.Append(sheet);
                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    List<String> columns = new List<String>();
                    foreach (System.Data.DataColumn column in table.Columns) {
                        columns.Add(column.ColumnName);
                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }//end loop
                    sheetData.AppendChild(headerRow);
                    foreach (System.Data.DataRow dsrow in table.Rows) {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns) {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }//end loop
                        sheetData.AppendChild(newRow);
                    }//end loop
                }//end loop
                workbookPart.Workbook.Save();
                workbook.Close();
            }//end using
            ms.Position = 0;
            return ms;
        }//end method

        private static List<char> Letters = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', ' ' };

        public static string GetColumnName(string cellReference) {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);

            return match.Value;
        }//end method

        public static int? GetColumnIndexFromName(string columnName) {
            int? columnIndex = null;

            string[] colLetters = Regex.Split(columnName, "([A-Z]+)");
            colLetters = colLetters.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (colLetters.Count() <= 2) {
                int index = 0;
                foreach (string col in colLetters) {
                    List<char> col1 = colLetters.ElementAt(index).ToCharArray().ToList();
                    int? indexValue = Letters.IndexOf(col1.ElementAt(index));

                    if (indexValue != -1) {
                        // The first letter of a two digit column needs some extra calculations
                        if (index == 0 && colLetters.Count() == 2) {
                            columnIndex = columnIndex == null ? (indexValue + 1) * 26 : columnIndex + ((indexValue + 1) * 26);
                        }
                        else {
                            columnIndex = columnIndex == null ? indexValue : columnIndex + indexValue;
                        }
                    }

                    index++;
                }
            }

            return columnIndex;
        }//end method

    }//end class
}//end namespace