using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;

namespace SurveyFileChecker
{
    public class CSVFileData
    {
        private int fID;
        private string fN;
        private string fP;
        private DateTime fDM;
        private Boolean lO;

        public CSVFileData()
        {
            
        }
        /// <summary>
        /// Class constructor when in most cases a new CSV is being loaded as File ID is auto-generated
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="fileDataModified"></param>
        /// <param name="LastOpened"></param>
        public CSVFileData(string fileName, string filePath, DateTime fileDataModified, Boolean LastOpened)
        {
            fN = fileName;
            fP = filePath;
            fDM = fileDataModified;
        }
        /// <summary>
        /// Class constructor when in most cases a CSV is being loaded from the file history and added to the workable collection
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="fileDataModified"></param>
        /// <param name="LastOpened"></param>
        public CSVFileData(int fileId, string fileName, string filePath, DateTime fileDataModified, Boolean LastOpened)
        {
            FID = fileId;
            fN = fileName;
            fP = filePath;
            fDM = fileDataModified;
        }

        #region get and set methods

        public int FileID { get => FID; set => FID = value; }

        public string FileName { get => fN; set => fN = value; }

        public string FilePath { get => fP; set => fP = value; }

        public DateTime FileDateModified { get => fDM; set => fDM = value; }
        
        public Boolean LastOpened { get => lO; set => lO = value; }
        public int FID { get => fID; set => fID = value; }

        #endregion

        /// <summary>
        /// Prints all csv file attributes to the debug console
        /// </summary>
        public void PrintAllData()
        {
            Debug.WriteLine("GETING ALL CSV FILE DATA...");
            Debug.WriteLine("CSV ID: " + FID + "\n" + "File Name: " + fN + "\n" + "File Path: " + fP + "\n" + "Date Modified: " + fDM.ToLongDateString() + "\n" + "Last Openeed: " + lO);
            Debug.WriteLine("");
        }

        //Returns a string to write to the File History txt file
        public string GenerateWritableString()
        {
            string[] sArr = { FID.ToString(), fN, fP, fDM.ToString(), lO.ToString()};
            return string.Join(",", sArr);
        }

        public DataTable GenerateDataSource()
        {
            DataTable data = new DataTable();

            ////
            //// NEW NEW NEW NEW
            ////

            ////Add the columns
            //File.ReadLines(fP).Take(1)
            //    .SelectMany(x => x.Split(',' ))
            //    .ToList()
            //    .ForEach(x => data.Columns.Add(x.Trim()));

            ////Add remaining rows
            //File.ReadLines(fP).Skip(1)
            //    .Select(x => x.Split(','))
            //    .ToList()
            //    .ForEach(line => data.Rows.Add(line));

            //return data;


            //OLD OLD OLD OLD

            string delimeters = ",";
            Boolean firstRowContainsFieldNames = true;
            int noOfFields = 0;

            try
            {
                using (TextFieldParser tfp = new TextFieldParser(fP))
                {
                    tfp.SetDelimiters(delimeters);
                    tfp.HasFieldsEnclosedInQuotes = true;
                  

                    if (!tfp.EndOfData)
                    {
                        string[] fields = tfp.ReadFields();
                        noOfFields = fields.Length;
                        //Write first line of CSV file as column titles
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (firstRowContainsFieldNames)
                                data.Columns.Add(fields[i]);
                            else
                                data.Columns.Add("Col " + i);
                        }
                        if (!firstRowContainsFieldNames)
                            data.Rows.Add(fields);

                    }
                    //Get remaining rows
                    int test = 0;
                    while (!tfp.EndOfData)
                    {
                        //var sb = new StringBuilder();

                        //string[] fields = tfp.ReadFields();

                        //for (int i = 0; i < fields.Length; i++)
                        //{
                        //    if (i == (noOfFields - 2))
                        //    {
                        //        string eString = fields[i];
                        //        eString.Replace(',','/');
                        //        sb.AppendLine(eString + ",");
                        //    }
                        //    else
                        //    {
                        //        sb.AppendLine(fields[i] + ",");
                        //        if (i == fields.Length)
                        //        {
                        //            sb.AppendLine(fields[i]);
                        //            break;
                        //        }
                        //    }
                        //}
                        //string[] output = sb.ToString().Split(',');
                        data.Rows.Add(tfp.ReadFields());
                    }
                }
                return data;
            }
            catch (Exception e)
            {
                //exception is thrown when the apploication is moved to a different computer (path) without a fresh intall
                Console.WriteLine(e);
                throw;
            }
        }
        #region CHECK FILE METHODS

        public virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        }

        #endregion

    }
}
