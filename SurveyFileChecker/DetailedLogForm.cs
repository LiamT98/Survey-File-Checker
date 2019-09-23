using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SurveyFileChecker
{
    public partial class DetailedLogForm : Form
    {
        private int entryID = 0;

        public DetailedLogForm()
        {
            InitializeComponent();

        }

        public void writeLogLines(List<string> lines)
        {
            foreach (var line in lines)
            {
                if (line.Contains("ERROR"))
                {
                    detailedLogText.SelectionFont = new Font(detailedLogText.Font, FontStyle.Bold);
                    detailedLogText.SelectionColor = Color.Red;
                    detailedLogText.AppendText("(" + entryID + "): " + line + "\n");
                    entryID++;
                }
                else
                {
                    detailedLogText.AppendText("(" + entryID + "): " + line + "\n");
                    entryID++;
                }
            }
        }
    }
}
