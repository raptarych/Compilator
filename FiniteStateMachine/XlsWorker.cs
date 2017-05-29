using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GemBox.Spreadsheet;

namespace FiniteStateMachine
{
    class XlsWorker
    {
        public Dictionary<string,Dictionary<string,string>> Read(string path)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            var excelFile = ExcelFile.Load(path);
            var workSheet = excelFile.Worksheets.FirstOrDefault();
            if (workSheet == null) return null;

            var table = new Dictionary<string, Dictionary<string, string>>();
            var columns = new List<string>();
            foreach (var row in workSheet.Rows)
            {
                var rowState = "";
                
                foreach (var cell in row.AllocatedCells)
                {
                    if (row.Index == 0)
                        if (cell.Column.Index > 0)
                            columns.Add(cell.StringValue);
                        else
                        {
                            continue;
                        }
                    else
                    {
                        if (cell.Column.Index == 0) table[cell.StringValue] = new Dictionary<string, string>();
                        else
                        {
                            var stateName = columns[cell.Column.Index - 1];
                            table[row.AllocatedCells[0].StringValue][stateName] = cell.StringValue;
                        }
                    }
                }
            }

            return table;
        }

        public bool Write(string path, Dictionary<string, Dictionary<string, string>> data)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            var success = true;
            try
            {
                var excelFile = new ExcelFile();
                var workSheet = excelFile.Worksheets.Add("FiniteStateModel");
                var rowNum = 0;
                workSheet.Cells[0, 0].Value = "State";
                var columnNum = 0;
                foreach (var key in data.FirstOrDefault().Value.Keys)
                {
                    columnNum++;
                    workSheet.Cells[0, columnNum].Value = key;
                }
                foreach (var rowKeyPair in data)
                {
                    rowNum++;
                    var row = rowKeyPair.Value;
                    columnNum = 0;
                    workSheet.Cells[rowNum, 0].Value = rowKeyPair.Key;
                    foreach (var col in row)
                    {
                        columnNum++;
                        workSheet.Cells[rowNum, columnNum].Value = col.Value;
                    }
                }
                excelFile.Save(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                success = false;
            }
            return success;
        }
    }
}
