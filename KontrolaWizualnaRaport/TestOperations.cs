using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    public class TestOperations
    {
        public static string testerIdToName(string lineID)
        {
            string testerID = "";
            switch (lineID)
            {
                case "1":
                    {
                        testerID = "Optical";
                        break;
                    }
                case "2":
                    {
                        testerID = "Manual-2";
                        break;
                    }
                case "3":
                    {
                        testerID = "Manual-1";
                        break;
                    }
                case "4":
                    {
                        testerID = "test_SMT5";
                        break;
                    }
                case "5":
                    {
                        testerID = "test_SMT6";
                        break;
                    }
                case "0":
                    {
                        testerID = "Splitting";
                        break;
                    }
            }
            return testerID;
        }

        public static Dictionary<string, Dictionary<string, List<double>>> CyCleTimePerMachinePerModelFamily (Dictionary<string, DataTable> InspectionRecordsPerMachine, Dictionary<string, string> lotModelDictionary)
        {
            Dictionary<string, Dictionary<string, List<double>>> result = new Dictionary<string, Dictionary<string, List<double>>>();
            int maxBreakDuration = 30;

            foreach (var machineEntry in InspectionRecordsPerMachine)
            {
                result.Add(machineEntry.Key, new Dictionary<string, List<double>>());
                DateTime prevDate = new DateTime(1700, 01, 01);
                foreach (DataRow row in machineEntry.Value.Rows)
                {
                    DateTime rowDate = (DateTime)row["data"];

                    if ((rowDate-prevDate).TotalMinutes>maxBreakDuration)
                    {
                        prevDate = rowDate;
                        continue;
                    }
                    string model = "";
                    if (!lotModelDictionary.TryGetValue(row["lot"].ToString(), out model)) continue;
                    if (!result[machineEntry.Key].ContainsKey(model))
                    {
                        result[machineEntry.Key].Add(model, new List<double>());
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, DataTable> InspectionRecordsPerMachine (Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData)
        {
            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();

            foreach (var dateEntry in testerData)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var machineEntry in shiftEntry.Value)
                    {
                        DataTable template = new DataTable();
                        template.Columns.Add("date", typeof(DateTime));
                        template.Columns.Add("id", typeof(string));
                        template.Columns.Add("lot", typeof(string));

                        if (!result.ContainsKey(machineEntry.Key))
                        {
                            result.Add(machineEntry.Key, template.Clone());
                        }

                        foreach (var lotEntry in machineEntry.Value)
                        {
                            foreach (DataRow row in lotEntry.Value.Rows)
                            {
                                DateTime date = DateTime.Parse(row["Data"].ToString());
                                string id = row["PCB"].ToString();
                                result[machineEntry.Key].Rows.Add(date, id, lotEntry.Key);
                            }
                        }
                    }
                }
            }


            return result;
        }

        public static void FillOutTesterTable(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> testerData, DataGridView grid, Dictionary<string, string> lotModelDictionary)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Tydzt", "Tydz");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("Optical", "Optical");
            grid.Columns.Add("Manual-1", "Manual-1");
            grid.Columns.Add("Manual-2", "Manual-2");
            grid.Columns.Add("test_SMT5", "test_SMT5");
            grid.Columns.Add("test_SMT6", "test_SMT6");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            foreach (var dateEntry in testerData)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }
                var week = dateTools.GetIso8601WeekOfYear(dateEntry.Key);
                foreach (var shiftEntry in dateEntry.Value)
                {
                    string date = dateEntry.Key.Date.ToString("yyyy-MM-dd");
                    string shift = shiftEntry.Key.ToString();

                    Dictionary<string, double> qtyPerMachine = new Dictionary<string, double>();
                    qtyPerMachine.Add("Optical", 0);
                    qtyPerMachine.Add("Manual-1", 0);
                    qtyPerMachine.Add("Manual-2", 0);
                    qtyPerMachine.Add("test_SMT5", 0);
                    qtyPerMachine.Add("test_SMT6", 0);


                    DataTable shiftTable = new DataTable();
                    shiftTable.Columns.Add("Data Start");
                    shiftTable.Columns.Add("Data Koniec");
                    shiftTable.Columns.Add("LOT");
                    shiftTable.Columns.Add("Model");
                    shiftTable.Columns.Add("Tester");
                    shiftTable.Columns.Add("Ilosc");
                    shiftTable.Columns.Add("Ilość cykli");
                    Dictionary<string, DataTable> tagPerMachine = new Dictionary<string, DataTable>();
                    tagPerMachine.Add("Optical", shiftTable.Clone());
                    tagPerMachine.Add("Manual-1", shiftTable.Clone());
                    tagPerMachine.Add("Manual-2", shiftTable.Clone());
                    tagPerMachine.Add("test_SMT5", shiftTable.Clone());
                    tagPerMachine.Add("test_SMT6", shiftTable.Clone());

                    foreach (var machineEntry in shiftEntry.Value)
                    {
                        if (!qtyPerMachine.ContainsKey(machineEntry.Key)) continue;
                        HashSet<string> pcbPerMachine = new HashSet<string>();
                        
                        foreach (var lotEntry in machineEntry.Value)
                        {
                            List<string> pcbPerLot = new List<string>();
                            DateTime start = DateTime.Now;
                            DateTime koniec = new DateTime(1970,1,1);
                            string model = "";
                            lotModelDictionary.TryGetValue(lotEntry.Key, out model);
                            
                            foreach (DataRow row in lotEntry.Value.Rows)
                            {
                                DateTime testDate = DateTime.Parse(row["Data"].ToString());
                                if (testDate > koniec) koniec = testDate;
                                if (testDate < start) start = testDate;
                                pcbPerMachine.Add(row["PCB"].ToString());
                                pcbPerLot.Add(row["PCB"].ToString());
                            }
                            tagPerMachine[machineEntry.Key].Rows.Add(start, koniec, lotEntry.Key, model, machineEntry.Key, pcbPerLot.Distinct().Count(), pcbPerLot.Count);
                        }

                        qtyPerMachine[machineEntry.Key] += pcbPerMachine.Count;
                    }
                    grid.Rows.Add(date, week, shift, qtyPerMachine["Optical"], qtyPerMachine["Manual-1"], qtyPerMachine["Manual-2"], qtyPerMachine["test_SMT5"], qtyPerMachine["test_SMT6"]);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.ColumnIndex > 2)
                        {
                            string tester = cell.OwningColumn.Name;
                            cell.Tag = tagPerMachine[tester];
                        }
                    }
                    
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}
