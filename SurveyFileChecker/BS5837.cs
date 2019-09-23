using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace SurveyFileChecker
{
    class BS5837 : CSVChecker
    {
        private DataGridView dataGridView;

        private string[] acceptedHeaders = {"Easting","Northing", "Height", "BS5837V111","Tree ID","Tag Number","TPO No","In Conservation Area","Tree Type","Common Name","Latin Name","Maturity","Likely Bat Habitat","Measurements Estimated","Height (m)","Height and direction of first significant branch (m)","Number of Stems","Stem 1 (mm) Enter average diameter for trees with more than 5 stems","Stem 2 (mm)","Stem 3 (mm)","Stem 4 (mm)","Stem 5 (mm)","Spread - N (m)","Spread - E (m)","Spread - S (m)","Spread - W (m)","CH - N (m)","CH - E (m)","CH - S (m)","CH - W (m)","Crown","Stem","Basal Area","Category","Life Expectancy","Subcategories","Phys Condition","Build Stage","Category 1","Action 1","Time 1","Category 2","Action 2","Time 2","Category 3","Action 3","Time 3","Category 4","Action 4","Time 4","Priority","Next Inspection (months)","Time","Date","Comment", "Image 1", "Image 2"};

        public BS5837(DataGridView dgv)
        {
            dataGridView = dgv;
        }

        public override void CheckAllColumns()
        {
            //Call to check header values
            checkHeaders();
            //Define method variables
            //Null category string
            string nullCat = "Category=";
            //string array containing values to be accepted by yes/no fields
            string[] yesNoArr = { "Yes", "yes", "y", "Y", "No", "no", "n", "N", "", " ", "Unknown" };
            //string list of TreeIDs
            List<string> treeIDList = new List<string>();

            foreach (DataGridViewColumn col in dataGridView.Columns)
            {
                if (col.Index == 44)
                    break;

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (row.Index == dataGridView.RowCount - 1)
                        break;

                    string cellStr = "";
                    string checkStr = "";
                    if (dataGridView[col.Index, row.Index].Value != null)
                        checkStr = dataGridView[col.Index, row.Index].Value.ToString();
                    if (checkStr == null)
                        cellStr = null;
                    else
                        cellStr = checkStr;

                    double cellDou = 0;
                    if (double.TryParse(cellStr, out double r))
                        cellDou = r;

                    Boolean hasHeightCol = false;

                    //Switch statement to use the correct check per the column
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
                        case "Height":
                            hasHeightCol = true;
                            if (cellStr != "")
                                if (!Decimal.TryParse(cellStr, out Decimal r3))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
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
                        case "In Conservation Area":
                        case "Likely Bat Habitat":
                        case "Measurements Estimated":
                            if (cellStr != "")
                                if (!Array.Exists(yesNoArr, element => element.Contains(cellStr)))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
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
                        //case "Tree Type":
                        //    if (!tables.isCellValid("Title=Genera", nullCat, cellStr))
                        //        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                        //    else if (cellStr == "")
                        //        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                        //    break;
                        //case "Latin Name":
                        //    if (cellStr != "" && cellStr != "- -" && cellStr != "#NAME?")
                        //    {
                        //        if (!tables.isCellValid("Title=Latin", dataGridView[8, row.Index].Value.ToString(), cellStr))
                        //            errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                        //    }
                        //    else if (cellStr == "#NAME?" || cellStr == "- -")
                        //        errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 0));
                        //    break;
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
                        //Stem stuff checks
                        case "Number of Stems":
                        //Height and direction of branch (m)
                        case "Height (m)":
                        case "Height and direction of first significant branch (m)":
                        case "Stem 1 (mm) Enter average diameter for trees with more that 5 stems":
                        case "Stem 2":
                        case "Stem 3":
                        case "Stem 4":
                        case "Stem 5":
                        case "Spread - N (m)":
                        case "Spread - E (m)":
                        case "Spread - S (m)":
                        case "Spread - W (m)":
                        case "CH - N (m)":
                        case "CH - E (m)":
                        case "CH - S (m)":
                        case "CH - W (m)":
                            if (cellStr != "")
                                if (!Decimal.TryParse(cellStr, out Decimal r2))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Crown and Basal condition
                        case "Crown":
                        case "Basal Area":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Crown and Basal", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                       //Stem condition
                        case "Stem":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Stem cond", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Category (A- F)
                        case "Category":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=RD", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Life expectancy
                        case "Life Expectancy":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Life Expectancy", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Sub categories
                        case "Subcategories":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Sub-Category", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Phys condition
                        case "Phys Condition":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Phys Cond", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Build stage
                        case "Build Stage":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Build Stage", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Category
                        case "Category 1":
                        case "Category 2":
                        case "Category 3":
                        case "Category 4":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Work Category", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Work items check
                        case "Action 1":
                        case "Action 2":
                        case "Action 3":
                        case "Action 4":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Work Items", dataGridView[col.Index - 1, row.Index].Value.ToString(), cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        case "Next Inspection":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Next Inspection", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Time check for work items
                        case "Time 1":
                        case "Time 2":
                        case "Time 3":
                        case "Time 4":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Time", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));  
                            break;
                        //Priority check
                        case "Priority":
                            if (cellStr != "")
                                if (!tables.isCellValid("Title=Priority", nullCat, cellStr))
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 1));
                            break;
                        //Time check
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
                        //Date check
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
                        //Comment check
                        case "Comment":
                            if (cellStr != "")
                                if (cellStr.Length > 255)
                                    errors.Add(GenerateError(row.Index, col.Index, col.HeaderText, 0));
                            break;
                    }
                }
            }
        }

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

        public override Boolean checkHeaders()
        {
            var headers = dataGridView.Columns.Cast<DataGridViewColumn>();
            int index = 0;
            Boolean hasFailed = false;

            foreach (DataGridViewTextBoxColumn col in headers)
            {
                if (col.HeaderText != acceptedHeaders[index])
                {
                    logLines.Add("ERROR IN HEADER NAME FOR " + col.HeaderText);
                    index++;
                    hasFailed = true;
                }
                else
                {
                    logLines.Add(col.HeaderText + " OKAY");
                    index++;
                }
            }

            if (!hasFailed)
            {
                logLines.Add("ALL HEADER VALUES OKAY");
                return true;
            }
            else
            {
                return false;
            }
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

        public override List<string> WriteLogLines()
        { 
            return logLines;
        }
    }
}
