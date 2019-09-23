using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace SurveyFileChecker
{
    public class Tables
    {
        private List<Table> tables = new List<Table>();

        public Tables()
        {
            
        }

        public string GetTableName(Table table) => table.name;

        public void populateTables(string p)
        {
            Table cTable = new Table();
            TableEntry cEntry = new TableEntry();

            using (var sr = new StreamReader(p))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (isFileStart(line))
                    {
                        while (sr.ReadLine() != "[End]") { }
                    }
                    else if (isTableStart(line))
                        cTable.name = sr.ReadLine();
                    else if (isTableEnd(line))
                    {
                        cTable.entries.Add(cEntry);
                        tables.Add(cTable);
                        cTable = new Table();
                        cEntry = new TableEntry();
                    }
                    else if (isTableCategory(line))
                    {
                        if (cEntry.items.Count() > 0)
                        {
                            cTable.entries.Add(cEntry);
                            cEntry = new TableEntry();
                        }
                        cEntry.category = line;
                    }
                    else if (isTableItem(line))
                        cEntry.items.Add(line);
                    else if (line == "[Topic]")
                        break;
                }
                //tables.Add(getNextTable(sr));
            }
        }
        Boolean isTableStart(string line)
        {
            if (line == "[Table]")
                return true;
            else
                return false;
        }
        Boolean isTableEnd(string line)
        {
            if (line == "[End]")
                return true;
            else
                return false;
        }
        Boolean isTableCategory(string line)
        {
            if (line.Contains("Category="))
                return true;
            else
                return false;    
        }
        Boolean isTableItem(string line)
        {
            if (line.Contains("Item="))
                return true;
            else
                return false;
        }
        Boolean isFileStart(string line)
        {
            if (line == "[Survey]")
                return true;
            else
                return false;
        }

        //REWRITE WITH JOHNS MEOTHD IN NOTE BOOK, READLINE CALLS ARE SHIT
        //private Table GetNextTable(StreamReader sr)
        //{
        //    Table tableToGet = new Table();
        //    string currentCategory;
        //    string line = sr.ReadLine();

        //    while (line != "[Table]")
        //    {
        //        line = sr.ReadLine();
        //    }
        //    tableToGet.name = sr.ReadLine();
        //    currentCategory = sr.ReadLine();
        //    line = "";
        //    while (line != "[End]")
        //    {
        //        TableEntry te = new TableEntry();
        //        te.items.Add(sr.ReadLine());
        //        while (!sr.ReadLine().Contains("Category"))
        //        {
        //            te.items.Add(line);
        //        }
        //        tableToGet.entries.Add(te);
        //    }     

        //    return tableToGet;
        //}
    public class Table
    {
        public string name;
        public List<TableEntry> entries = new List<TableEntry>();
    }
    public class TableEntry
    {
        public string category;
        public List<string> items = new List<string>();
    }

    public void temp_getLatinUnknowns()
    {
        Table tableToGet = new Table();
        tableToGet = tables.First(t => t.name == "Title=Latin");
        foreach (TableEntry ent in tableToGet.entries)
        {
            int index = 0;
            while (index < ent.items.Count)
            {
                if (ent.items[index].Contains("Unknown"))
                {
                    Debug.WriteLine(ent.items[index]);
                    index++;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    public Boolean isCellValid(string name, string category, string item)
    {
        if (category != "Category=")
            category = "Category=" + category;
        item = "Item=" + item;
        Table tableToGet = new Table();
        tableToGet = tables.First(t => t.name == name);
        foreach (TableEntry ent in tableToGet.entries.Where(c => c.category == category))
        {
           if (ent.items.Contains(item))
                return true;
            else
                return false;
        }
        return false;
        }
}
}
