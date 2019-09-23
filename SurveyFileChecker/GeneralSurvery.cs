using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using Infragistics.Win.UltraWinGrid;
using System.Globalization;

namespace SurveyFileChecker
{
    class GeneralSurvery : CSVChecker
    {
        private DataGridView dataGridView;
        //string array of header values for General Survery headings 
        private string[] acceptedHeaders = { "Easting", "Northing", "Height", "GMSurveyV111", "Tree ID", "Tag No", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Stems", "Height (m)", "Stem Dia (mm)", "Spread Radius (m)", "Maturity", "Bat Habitat", "Overall", "Branches", "Leaf/Buds", "Roots", "Stem", "Work Category 1", "Work Item 1", "Time 1", "Priority 1", "Cost 1", "Work Category 2", "Work Item 2", "Time 2", "Priority 2", "Cost 2", "Work Category 3", "Work Item 3", "Time 3", "Priority 3", "Cost 3", "Work Category 4", "Work Item 4", "Time 4", "Priority 4", "Cost 4", "Next Survey (months)", "QTRA Base Score 1/", "Time", "Date", "Comment", "Image 1", "Image 2" };

        public GeneralSurvery(DataGridView dgv)
        {
            dataGridView = dgv;
        }
        //
        //NOTES
        // Line to check header names against table in check file
        // tables.isCellValid("Title=Headers", nullCat, dataGridView[col.Index, rowIndex].Value.ToString());
        //

        public override void CheckAllColumns()
        {
            //Check DataGridView headers independently
            checkHeaders();
            errorListLines.Add("-----------------------------------------");
            //string to pass a null category value to correctly search Tables. Otherwise category value would be null
            string nullCat = "Category=";
            //string array to store values to check certain fields against
            string[] yesNoArr = { "Yes", "yes", "y", "Y", "No", "no", "n", "N", "", " ", "Unknown" };
            //Define string list to store Tree IDs
            List<string> treeIDList = new List<string>();

            foreach (DataGridViewColumn col in dataGridView.Columns)
            {
                //Force end the check if the column index equals the number of columns
                if (col.Index == 46)
                    break;

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    //Force next column reading if the row.Index equals the number of rows
                    if (row.Index == dataGridView.RowCount - 1)
                        break;

                    //Pass target cell value as a string to avoid statement conversion
                    //If the cell is empty, assign null value for if conditiion compatibility
                    string cellStr = "";
                    string checkStr = "";
                    if (dataGridView[col.Index, row.Index].Value != null)
                        checkStr = dataGridView[col.Index, row.Index].Value.ToString();
                    if (checkStr == null)
                        cellStr = null;
                    else
                        cellStr = checkStr;
                    //Pass target cell value as a double to avoid statement conversion
                    double cellDou = 0;
                    if (double.TryParse(cellStr, out double r))
                    {
                        cellDou = r;
                    }
                    //
                    //CHECK VALUES SWTICH STATEMENT. CHECKS COLUMN BY COLUMN
                    //FIELDS THAT AREN'T CHECKED
                    // - Tag No
                    // - TPO No
                    // - Work Category 1 - 4
                    // - Work Item 1 - 4
                    //
                    Boolean hasHeightCol = false;

                    switch (col.HeaderText)
                    {
                        //Easting Value Check
                        case "Easting":
                            if (cellDou < 0 || cellDou > 665000 || cellStr == "")
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Northing value check
                        case "Northing":
                            if (cellDou < 0 || cellDou > 12000000 || cellStr == "")
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //TreeID value check
                        case "Tree ID":
                            if (cellStr == "")
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            else
                            {
                                if (treeIDList.Any(item => item.Equals(cellStr)))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                                else
                                    treeIDList.Add(cellStr);
                            }
                            break;
                        case "GMSurveyv111":
                        break;
                        //Check height value
                        case "Height":
                            hasHeightCol = true;
                            if (cellStr != "")
                                if (!Decimal.TryParse(cellStr, out Decimal r3))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Stem value check
                        case "Stems":
                            if (cellStr != "")
                                if (!Int32.TryParse(cellStr, out int r1))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        // In conservation area and Bat Habitat check
                        case "In Conservation Area":
                        case "Bat Habitat":
                            if (cellStr != "")
                                if (!Array.Exists(yesNoArr, element => element.Contains(cellStr)))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Tree type check. This must be value
                        case "Tree Type":
                            if (!tables.isCellValid("Title=Genera", nullCat, cellStr))
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            else if (cellStr == "")
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        case "Common Name":
                            if (cellStr != "")
                            {
                                if (hasHeightCol == true)
                                    if (!tables.isCellValid("Title=Common name", dataGridView[8, row.Index].Value.ToString(), cellStr))
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                                else
                                    if (!tables.isCellValid("Title=Common name", dataGridView[7, row.Index].Value.ToString(), cellStr))
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            }
                            break;
                        //Latin name check. Uses the Common Name as the category to search table items
                        case "Latin Name":
                            if (cellStr != "" && cellStr != "- -" && cellStr != "#NAME?")
                            {
                                if (hasHeightCol == true)
                                    if (!tables.isCellValid("Title=Latin", dataGridView[9, row.Index].Value.ToString(), cellStr))
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                                else
                                    if (!tables.isCellValid("Title=Latin", dataGridView[8, row.Index].Value.ToString(), cellStr))
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            }
                            else if (cellStr == "#NAME?" || cellStr == "- -")
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 0));
                            break;
                        //Height (m), Stem Dia (mm) and Spread Radius (m) value check
                        case "Height (m)":
                        case "Stem Dia (mm)":
                        case "Spread Radius (m)":
                            if (cellStr != "")
                                if (!Decimal.TryParse(cellStr, out Decimal r2))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Maturity value check
                        case "Maturity":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Maturity", nullCat, cellStr))
                                {
                                    if (cellStr == "Veteran")
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 0));
                                    else
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                                }
                            break;
                        //Overall Condition value check
                        case "Overall":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Overall Condition", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 0));
                            break;
                        //Condition Branches value check
                        case "Branches":
                            if (cellStr != "")
                                if (CheckMultiItemCell("Title=Condition Branches", cellStr) == false)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Condition of Leaf and Buds value check
                        case "Leaf/Buds":
                            if (cellStr != "")
                                if (CheckMultiItemCell("Title=Condition Leaf and Buds", cellStr) == false)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Condition Roots value check
                        case "Roots":
                            if (cellStr != "")
                                if (CheckMultiItemCell("Title=Condition Roots", cellStr) == false)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));

                            break;
                        //Condition Stem value check
                        case "Stem":
                            if (cellStr != "")
                                if (CheckMultiItemCell("Title=Condition Stem", cellStr) == false)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        case "Work Category 1":
                        case "Work Category 2":
                        case "Work Category 3":
                        case "Work Category 4":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Work Category",nullCat, cellStr))
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Work item Time Value check
                        case "Time 1":
                        case "Time 2":
                        case "Time 3":
                        case "Time 4":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Time", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Work item Priority value check
                        case "Priority 1":
                        case "Priority 2":
                        case "Priority 3":
                        case "Priority 5":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Priority", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Cost value check
                        case "Cost 1":
                        case "Cost 2":
                        case "Cost 3":
                        case "Cost 4":
                            if (cellStr != "")
                                if (!Int32.TryParse(cellStr, out int r3))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //QTRA value check
                        case "QTRA Base Scire 1/":
                            if (cellStr != "")
                                if (cellDou < 0 || cellDou > 1000000)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Time value check
                        case "Time":
                            if (cellStr != "" && cellStr != "0")
                            {
                                if (DateTime.TryParse(cellStr, out DateTime dt))
                                    break;
                                else
                                {
                                    if (cellStr.Length == 5)
                                        cellStr = "0" + cellStr;
                                    char[] cArr = cellStr.ToCharArray();
                                    string timeToParse = "" + cArr[0] + cArr[1] + ":" + cArr[2] + cArr[3] + ":" + cArr[4] + cArr[5];
                                    if (!DateTime.TryParse(timeToParse, out DateTime dt1))
                                        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                                }
                            }
                            break;
                        //Date value check
                        case "Date":
                            if (cellStr == "")
                                errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            else if (!DateTime.TryParse(cellStr, out DateTime r5))
                            {
                                string date = DateTime.ParseExact(cellStr, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy/MM/dd");
                                if (!DateTime.TryParse(date, out DateTime r6))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));   
                            }
                            break;
                        //Comment value check
                        case "Comment":
                            if (cellStr != "")
                                if (cellStr.Length > 255)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 0));
                            break;
                    }
                }
            }
        }
        //Method for checking a cell value with multiple items split with a ';' character
        public Boolean CheckMultiItemCell(string tableName, string cell)
        {
            string nullCat = "Category=";
            string[] strArr = cell.Split(';');

            for (int i = 0; i < strArr.Length - 1; i++)
            {
                if (!tables.isCellValid(tableName, nullCat, strArr[i]))
                    return false;
            }

            return true;
        }

        //Method to check the headers of the DataGridView agaisnt acceptable headers array
        public override Boolean checkHeaders()
        {
            var headers = dataGridView.Columns.Cast<DataGridViewColumn>();
            int index = 0;
            Boolean hasFailed = false;

            foreach (DataGridViewTextBoxColumn col in headers)
            {
               if (col.HeaderText != acceptedHeaders[index])
                {
                    errors.Add(GenerateError(0, index, col.HeaderText, 2));
                    
                    hasFailed = true;
                    index++;
                    break;
                }
                index++;
            }
            if (!hasFailed)
            {
                logLines.Add("ALL HEADER VALUES OKAY");
                return true;
            }
            else
                return false;
        }

        public Error GenerateError(int x, int y, string colHeader, int type)
        {
            int eNo = errors.Count + 1;
            Error eToReturn = new Error();
            eToReturn.ErrorNumber = eNo;
            eToReturn.ErrorType = type;
            eToReturn.AffectedColumn = colHeader;
            eToReturn.LineNumber = x + 2;

            eToReturn.X = x;
            eToReturn.Y = y;

            return eToReturn;

        }

        //Method to write log lines to detailed log
        // (errors and program status information)
        public override List<string> WriteLogLines()
        {
            return logLines;
        }
    }
}
