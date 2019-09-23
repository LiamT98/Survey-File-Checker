using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CsvHelper;
using Infragistics.Win.UltraWinTabs;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;

namespace SurveyFileChecker
{
    public partial class Form1 : Form
    { 
        Collection<CSVFileData> csvFiles = new Collection<CSVFileData>();

        CSVChecker csvToCheck;

        DetailedLogForm logForm = new DetailedLogForm();

        //private string BS5837V111_TablePath = Path.Combine("Resources", "BS5837 Survey V111.txt");
        private string BS5837V111_TablePath = Application.StartupPath + @"\Resources\BS5837 Survey V111.txt";
        //private string GeneralSurveyV111_TablePath = Path.Combine("Resources", "General Survey V111.txt");
        private string GeneralSurveyV111_TablePath = Application.StartupPath + @"\Resources\General Survey V111.txt";
        //private string GeneralSurveyV111_TablePath = global::SurveyFileChecker.Properties.Resources.General_Survey_V111;


        private string[] acceptedHeaders = { "Easting", "Northing", "GMSurveyV111", "Tree ID", "Tag No", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Stems", "Height (m)", "Stem Dia (mm)", "Spread Radius (m)", "Maturity", "Bat Habitat", "Overall", "Branches", "Leaf/Buds", "Roots", "Stem", "Work Category 1", "Work Item 1", "Time 1", "Priority 1", "Cost 1", "Work Category 2", "Work Item 2", "Time 2", "Priority 2", "Cost 2", "Work Category 3", "Work Item 3", "Time 3", "Priority 3", "Cost 3", "Work Category 4", "Work Item 4", "Time 4", "Priority 4", "Cost 4", "Next Survey (months)", "QTRA Base Score 1/", "Time", "Date", "Comment" };
        private string[] acceptedHeaders2 = { "Easting", "Northing", "BS5837V111", "Tree ID", "Tag Number", "TPO No", "In Conservation Area", "Tree Type", "Common Name", "Latin Name", "Maturity", "Likely Bat Habitat", "Measurements Estimated", "Height (m)", "Height and direction of first significant branch (m)", "Number of Stems", "Stem 1 (mm) Enter average diameter for trees with more than 5 stems", "Stem 2 (mm)", "Stem 3 (mm)", "Stem 4 (mm)", "Stem 5 (mm)", "Spread - N (m)", "Spread - E (m)", "Spread - S (m)", "Spread - W (m)", "CH - N (m)", "CH - E (m)", "CH - S (m)", "CH - W (m)", "Crown", "Stem", "Basal Area", "Category", "Life Expectancy", "Subcategories", "Phys Condition", "Build Stage", "Category 1", "Action 1", "Time 1", "Category 2", "Action 2", "Time 2", "Category 3", "Action 3", "Time 3", "Category 4", "Action 4", "Time 4", "Priority", "Next Inspection (months)", "Time", "Date", "Comment" };

        public Form1()
        {
            InitializeComponent();

            Text = "Survey File Checker - V" + Application.ProductVersion+" [ALPHA]";

            Error e = new Error();

            exportNewCSVToolStripMenuItem.Visible = false;

            if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
            {
                Type dgvType = dataGridView1.GetType();
                PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(dataGridView1, true, null);
            }
        }

        void temp_genTableString()
        {
            for (int i = 0; i < acceptedHeaders2.Length; i++)
            {
                Debug.WriteLine("Item=" + acceptedHeaders2[i]);
            }
        }

        void LoadNewFile(string path)
        {
            ClearErrors();
            FileInfo fi = new FileInfo(path);
            CSVFileData csvToAdd = new CSVFileData(fi.Name, fi.FullName, fi.LastWriteTime, true);
            csvFiles.Add(csvToAdd);

            dataGridView1.DataSource = csvToAdd.GenerateDataSource();

            Text = fi.Name;
            exportCSVStripMenuItem.Visible = true;
        }

        Boolean IsFHIValid(string pathToCheck)
        {
            //Check if given file exists and is empty
            //Returns bool
            FileInfo fi = new FileInfo(pathToCheck);
            if (fi.Exists == true)
            {
                //Check if file is eempty
                if (new FileInfo(pathToCheck).Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //If file doesn't exist, create file, returning false as the file is empty
                using (FileStream fs = File.Create(pathToCheck))
                {
                    return false;
                }
            }
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        { 
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            errorList.Items.Clear();
            var sb = new StringBuilder();
            var headers = dataGridView1.Columns.Cast<DataGridViewColumn>();

            sb.Append(string.Join(",", headers.Select(column => column.HeaderText)));

            if (string.Join(",", headers.Select(column => column.HeaderText)).Contains("GMSurveyV111"))
            {
                csvToCheck = new GeneralSurvery(dataGridView1);
                csvToCheck.errors.Clear();
                csvToCheck.tables.populateTables(GeneralSurveyV111_TablePath);
                csvToCheck.CheckAllColumns();
                if (csvToCheck.errors.Count == 0)
                {
                    var msgBox = MessageBox.Show("No errors found!", "Valid File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    WriteErrorList(csvToCheck.errors);
                    highlightBtn.Visible = true;
                }
            }
            else if (string.Join(",", headers.Select(column => column.HeaderText)).Contains("BS5837V111"))
            {
                csvToCheck = new BS5837(dataGridView1);
                csvToCheck.errors.Clear();
                csvToCheck.tables.populateTables(BS5837V111_TablePath);
                csvToCheck.CheckAllColumns();
                if (csvToCheck.errors.Count == 0)
                {
                    var msgBox = MessageBox.Show("No errors found!", "Valid File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    WriteErrorList(csvToCheck.errors);
                    highlightBtn.Visible = true;
                }
            }
            
           
        }

        private void WriteErrorList(List<Error> eToWrite)
        {
            int warningCount = 0;
            int errorCount = 0;
            foreach(Error e in eToWrite)
            {
                ListViewItem lvi = new ListViewItem(e.ErrorNumber.ToString());
                lvi.SubItems.Add(e.GetErrorType());
                lvi.SubItems.Add(e.LineNumber.ToString());
                lvi.SubItems.Add(e.AffectedColumn);
                lvi.SubItems.Add("");
                lvi.UseItemStyleForSubItems = false;

                if (e.ErrorType == 1)
                {
                    lvi.SubItems[1].BackColor = Color.Tomato;
                    errorCount++;
                }
                else if (e.ErrorType == 0)
                {
                    lvi.SubItems[1].BackColor = Color.Gold;
                    warningCount++;
                }
                else
                {
                    lvi.SubItems[2].Text = "Header";
                    lvi.SubItems[4].Text = "Open CSV in excel to see invalid column header";
                    lvi.SubItems[1].BackColor = Color.Blue;
                    errorCount++;
                }
                    
                errorList.Items.Add(lvi);
            }
            label1.Text = errorCount + " Errors";
            label2.Text = warningCount + " Warnings";
        }

        private void DetailedLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logForm.Show();
        }

        private void DataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(dataGridView1.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 2).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 4);
            }
        }

        private void ErrorList_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            Error err = new Error();
            err = csvToCheck.errors.FirstOrDefault(x => x.ErrorNumber == Int32.Parse(errorList.FocusedItem.SubItems[0].Text));
            if (err.ErrorType == 1)
                dataGridView1[err.Y, err.X].Style.BackColor = Color.Tomato;
            else if (err.ErrorType == 0)
                dataGridView1[err.Y, err.X].Style.BackColor = Color.Gold;
            else
                return;

            dataGridView1.CurrentCell = dataGridView1[err.Y, err.X];
        }

        private void ClearErrors()
        {
            errorList.Items.Clear();
            label1.Text = "";
            label2.Text = "";
        }

        private void OpenStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = @"C:\..\";
            ofd.Title = "Browse CSV files";

            ofd.CheckFileExists = true;
            ofd.Multiselect = false;

            ofd.DefaultExt = "csv";
            ofd.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            ofd.ShowDialog();

            ofd.RestoreDirectory = true;

            if (ofd.FileName != "")
                LoadNewFile(ofd.FileName);
        }

        private void ExportCSVStripMenuItem_Click(object sender, EventArgs e)
        {
            //NEW
            string exportPath = @"Exports\";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = exportPath;
            sfd.DefaultExt = "*.csv";
            sfd.FileName = csvFiles[csvFiles.Count - 1].FileName;
            sfd.Filter = "CSV files|*.csv|Text files|*.txt|All file types|*.";
            sfd.Title = "Export new CSV";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var sb = new StringBuilder();

                var headers = dataGridView1.Columns.Cast<DataGridViewColumn>();
                sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>();
                    sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
                }

                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    sw.Write(sb);
            }
        }

        private void HighlightBtn_Click(object sender, EventArgs e)
        {
            foreach (Error err in csvToCheck.errors)
            {
                if (err.ErrorType == 1)
                    dataGridView1[err.Y, err.X].Style.BackColor = Color.Tomato;
                else
                    dataGridView1[err.Y, err.X].Style.BackColor = Color.Gold;
            }
        }

        private void DataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (file.Length == 1)
            {
                LoadNewFile(file[0]);
            }
            else
            {
                var msgBox = MessageBox.Show("Please only drag one file at a time", "Invalid file drop, multiple files detected!", MessageBoxButtons.OK);
            }
        }
        private void openFileDrop()
        {

        }
    }
}
