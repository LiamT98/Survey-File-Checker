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

namespace SurveyFileChecker
{
    class GeneralSurvery : CSVChecker
    {
        private DataGridView dataGridView;
        //string array of header values for General Survery headings 
        private string[] acceptedHeaders = { "Easting", "Northing", "Height", "GMSurveyV111", "Tree ID", "Tag No", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Stems", "Height (m)", "Stem Dia (mm)", "Spread Radius (m)", "Maturity", "Bat Habitat", "Overall", "Branches", "Leaf/Buds", "Roots", "Stem", "Work Category 1", "Work Item 1", "Time 1", "Priority 1", "Cost 1", "Work Category 2", "Work Item 2", "Time 2", "Priority 2", "Cost 2", "Work Category 3", "Work Item 3", "Time 3", "Priority 3", "Cost 3", "Work Category 4", "Work Item 4", "Time 4", "Priority 4", "Cost 4", "Next Survey (months)", "QTRA Base Score 1/", "Time", "Date", "Comment" };

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
            //string to pass a null category value to correctly search Tables. Otherwise category value would be null
            string nullCat = "Category=";
            //string array to store values to check certain fields against
            string[] yesNoArr = { "Yes", "yes", "y", "Y", "No", "no", "n", "N", "", " ", "Unknown" };
            //Define string list to store Tree IDs
            List<string> treeIDList = new List<string>();

            foreach (DataGridViewColumn col in dataGridView.Columns)
            {
                //Force end the check if the column index equals the number of columns
                if (col.Index == 44)
                    break;


                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    //Force next column reading if the row.Index equals the number of rows
                    if (row.Index == dataGridView.RowCount - 1)
                        break;

                    //Pass target cell value as a string to avoid statement conversion
                    //If the cell is empty, assign null value for if conditiion compatibility
                    string cellStr = dataGridView[col.Index, row.Index].Value.ToString();
                    if (cellStr == "")
                        cellStr = null;

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
                    switch (col.Index)
                    {
                        //Easting Value Check
                        case 0:
                            if (cellDou < 0 || cellDou > 600000 || cellStr == null)
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Northing value check
                        case 1:

                            if (cellDou < 0 || cellDou > 12000000 || cellStr == null)
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //TreeID value check
                        case 3:
                            if (cellStr == null)
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            else
                            {
                                if (treeIDList.Any(item => item.Equals(cellStr)))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                                else
                                    treeIDList.Add(cellStr);
                            }
                            break;
                        //Stem value check
                        case 10:
                            if (cellStr != null)
                                if (!Int32.TryParse(cellStr, out int r1))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        // In conservation area and Bat Habitat check
                        case 6:
                        case 15:
                            if (cellStr != null)
                                if (!Array.Exists(yesNoArr, element => element.Contains(cellStr)))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Tree type check. This must be value
                        case 7:
                            if (!tables.isCellValid("Title=Genera", nullCat, cellStr))
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            else if (cellStr == null)
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Latin name check. Uses the Common Name as the category to search table items
                        case 9:
                            if (cellStr != null && cellStr != "- -" && cellStr != "#NAME?")
                            {
                                if (!tables.isCellValid("Title=Latin", dataGridView[8, row.Index].Value.ToString(), cellStr))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            }
                            else if (cellStr == "#NAME?" || cellStr == "- -")
                                errorListLines.Add(generateErrorLine('w', col.HeaderText, row.Index));
                            break;
                        //Height (m), Stem Dia (mm) and Spread Radius (m) value check
                        case 11:
                        case 12:
                        case 13:
                            if (cellStr != null)
                                if (!cellStr.Any(c => char.IsDigit(c)))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Maturity value check
                        case 14:
                            if (cellStr != null)
                                if (!tables.isCellValid("Title=Maturity", nullCat, cellStr))
                                {
                                    if (cellStr == "Veteran")
                                        errorListLines.Add(generateErrorLine('w', col.HeaderText, row.Index));
                                    else
                                        errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                                }

                            break;
                        //Overall Condition value check
                        case 16:
                            if (cellStr != null)
                                if (!tables.isCellValid("Title=Overall Condition", nullCat, cellStr))
                                    errorListLines.Add(generateErrorLine('w', col.HeaderText, row.Index));
                            break;
                        //Condition Branches value check
                        case 17:
                            if (cellStr != null)
                                if (checkMultiItemCell("Title=Condition Branches", cellStr) == false)
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Condition of Leaf and Buds value check
                        case 18:
                            if (cellStr != null)
                                if (checkMultiItemCell("Title=Condition Leaf and Buds", cellStr) == false)
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Condition Roots value check
                        case 19:
                            if (cellStr != null)
                                if (checkMultiItemCell("Title=Condition Roots", cellStr) == false)
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));

                            break;
                        //Condition Stem value check
                        case 20:
                            if (cellStr != null)
                                if (checkMultiItemCell("Title=Condition Stem", cellStr) == false)
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Work item Time Value check
                        case 23:
                        case 28:
                        case 33:
                        case 38:
                            if (cellStr != null)
                                if (!tables.isCellValid("Title=Time", nullCat, cellStr))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Work item Priority value check
                        case 24:
                        case 29:
                        case 34:
                        case 39:
                            if (cellStr != null)
                                if (!tables.isCellValid("Title=Priority", nullCat, cellStr))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Cost value check
                        case 41:
                            if (cellStr == null)
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            else if (cellStr != null)
                                if (!Int32.TryParse(cellStr, out int r3))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //QTRA value check
                        case 42:
                            if (cellStr != null)
                                if (cellDou < 0 || cellDou > 1000000)
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Time value check
                        case 43:
                            if (cellStr != null)
                            {
                                char[] cArr = cellStr.ToCharArray();
                                string timeToParse = "" + cArr[0] + cArr[1] + ":" + cArr[2] + cArr[3] + ":" + cArr[4] + cArr[5];
                                if (!DateTime.TryParse(timeToParse, out DateTime dt))
                                    errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            }      
                            break;
                        //Date value check
                        case 44:
                            if (cellStr == null)
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            else if (!DateTime.TryParse(cellStr, out DateTime r5))
                                errorListLines.Add(generateErrorLine('e', col.HeaderText, row.Index));
                            break;
                        //Comment value check
                        case 45:
                            if (cellStr != null)
                                if (cellStr.Length > 255)
                                    errorListLines.Add(generateErrorLine('w', col.HeaderText, row.Index));
                            break;
                    }
                }

            }
        }
        //Method for checking a cell value with multiple items split with a ';' character
        public Boolean checkMultiItemCell(string tableName, string cell)
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
                if (col.Index == 44)
                    break;
                if (col.HeaderText != acceptedHeaders[index])
                {
                    errorListLines.Add("ERROR IN HEADER NAME FOR " + col.HeaderText);
                    index++;
                    hasFailed = true;
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

        //Method to generate string to write to error list text box
        public string generateErrorLine(char lineType, string columnName, int lineNumber)
        {
            lineNumber = lineNumber + 2;
            if (lineType == 'e')
                return "ERROR | INVALID " + columnName + " VALUE | LINE: " + lineNumber;
            else if (lineType == 'w')
                return "WARNING | POTENTIAL ERRONEOUS " + columnName + " VALUE | LINE: " + lineNumber;
            else
                return "APPLICATION ERROR | CANNOT GENERATE ERROR MESSAGE | CONTACT LIAM TAYOR @ PEAR TECHNOLOGY";
        }

        //Method to write log lines to detailed log
        // (errors and program status information)
        public override List<string> WriteLogLines()
        {
            return logLines;
        }
    }
}
