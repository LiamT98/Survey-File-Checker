using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SurveyFileChecker
{
    public abstract class CSVChecker
    {
        public List<Error> errors = new List<Error>();

        public List<string> errorListLines = new List<string>();

        public List<string> logLines = new List<string>();

        public string path;

        public Tables tables = new Tables();

        public CSVChecker()
        {

        }
        public abstract Boolean checkHeaders();

        //CHECK ALL
        public abstract void CheckAllColumns();

        public abstract List<string> WriteLogLines();

        public List<string> ReturnErrorLines()
        {
            return errorListLines;
            //return errorsToReturn;
        }

        public List<string> ReturnLogLines()
        {
            return logLines;
        }

       




    }
}
