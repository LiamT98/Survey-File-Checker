using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveyFileChecker
{
    public class Error
    {
        private int errorNum;
        private int errorType;
        private string colHeader;
        private int lineNo;
        private int x;
        private int y;

        public Error()
        {

        }
        public Error(int eN, int eT, string cH, int lN, int x, int y)
        {
            ErrorNumber = eN;
            ErrorType = eT;
            AffectedColumn = cH;
            LineNumber = lN;
            X = x;
            Y = y;
        }
        public int ErrorNumber { get => errorNum; set => errorNum = value; }

        public int ErrorType { get => errorType; set => errorType = value; }

        public string AffectedColumn { get => colHeader; set => colHeader = value; }

        public int LineNumber { get => lineNo; set => lineNo = value; }

        public int X { get => x; set => x = value; }

        public int Y { get => y; set => y = value; }

        public string[] GetErrorLine()
        {
            string type;
            if (ErrorType == 0)
                type = "WARNING";
            else if (ErrorType == 1)
                type = "ERROR";
            else
                type = "HEADER ERROR";

            string[] strReturn = new string[] { ErrorNumber.ToString(), type, LineNumber.ToString(), AffectedColumn};

            return strReturn;
        }
        public string GetErrorType()
        {
            string type;
            if (ErrorType == 0)
                type = "WARNING";
            else if (ErrorType == 1)
                type = "ERROR";
            else
                type = "HEADER ERROR";
            return type;
        }


    }
}
