using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class VIOperations
    {
        public static Dictionary<string, string>[] lotArray(DataTable lotTable)
        {
            Dictionary<string, string> result1 = new Dictionary<string, string>();
            Dictionary<string, string> result2 = new Dictionary<string, string>();
            Dictionary<string, string> result3 = new Dictionary<string, string>();
            Dictionary<string, string> result4 = new Dictionary<string, string>();

            foreach (DataRow row in lotTable.Rows)
            {
                if (result1.ContainsKey(row["Nr_Zlecenia_Produkcyjnego"].ToString())) continue;
                result1.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["NC12_wyrobu"].ToString().Replace("LLFML", ""));
                result2.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["Ilosc_wyrobu_zlecona"].ToString());
                result3.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["LiniaProdukcyjna"].ToString());
            }
            return new Dictionary<string, string>[] { result1, result2, result3, result4 };
        }

        public static void ngRatePerOperator(List<WasteDataStructure> inspectionData, DateTime startDate, DateTime endDate, DataGridView grid)
        {
            grid.Columns.Clear();
            
            Dictionary<string, List<WasteDataStructure>> inspectionDataPerOperator = inspectionData.GroupBy(op => op.Oper).ToDictionary(op => op.Key, op => op.ToList());

            //result.Columns.Add("Operator");
            //result.Columns.Add("Sprawdzone", typeof (double));
            //result.Columns.Add("Śr.na zmianę", typeof (double));
            //result.Columns.Add("h/zmiane", typeof (double));
            //result.Columns.Add("śr/h", typeof (double));
            //result.Columns.Add("NG", typeof(double));
            //result.Columns.Add("NG%", typeof(double));
            //result.Columns.Add("Scrap", typeof(double));
            //result.Columns.Add("Scrap%", typeof(double));

            grid.Columns.Add("Operator", "Operator");
            grid.Columns.Add("Sprawdzone", "Sprawdzone");
            grid.Columns.Add("Śr.na zmianę", "Śr.na zmianę");
            grid.Columns.Add("h/zmiane","h/zmiane");
            grid.Columns.Add("na godz.", "na godz.");
            grid.Columns.Add("NG", "NG");
            grid.Columns.Add("NG%", "NG%");
            grid.Columns.Add("Scrap", "Scrap");
            grid.Columns.Add("Scrap%", "Scrap % ");

           
            foreach (var operatorEntry in inspectionDataPerOperator)
            {
                //double totalInspected = operatorEntry.Value.Select(t => t.AllQty).Sum();
                //double totalNg = operatorEntry.Value.Select(t => t.AllNg).Sum();
                //double ngPercent = Math.Round(totalNg / totalInspected * 100, 2);
                //double totalScrap = operatorEntry.Value.Select(t => t.AllScrap).Sum();
                //double scrapPercent = Math.Round(totalScrap / totalInspected * 100, 2);
                Dictionary<DateTime, Dictionary<string, List<WasteDataStructure>>> operatorperDay = new Dictionary<DateTime, Dictionary<string, List<WasteDataStructure>>>();

                double totalInspected = 0;
                double totalNg = 0;
                double totalScrap = 0;
                HashSet<DateTime> daysOfWork = new HashSet<DateTime>();
                
                
                foreach (var wasteEntry in operatorEntry.Value)
                {
                    if (wasteEntry.FixedDateTime.Date < startDate.Date || wasteEntry.FixedDateTime.Date > endDate.Date) continue;
                    totalInspected += wasteEntry.AllQty;
                    totalNg += wasteEntry.AllNg;
                    totalScrap += wasteEntry.AllScrap;
                    daysOfWork.Add(wasteEntry.FixedDateTime.Date);

                    if(!operatorperDay.ContainsKey(wasteEntry.FixedDateTime.Date))
                    {
                        operatorperDay.Add(wasteEntry.FixedDateTime.Date, new Dictionary<string, List<WasteDataStructure>>());
                    }
                    if (!operatorperDay[wasteEntry.FixedDateTime.Date].ContainsKey(wasteEntry.Model))
                    {
                        operatorperDay[wasteEntry.FixedDateTime.Date].Add(wasteEntry.Model, new List<WasteDataStructure>());
                    }
                    operatorperDay[wasteEntry.FixedDateTime.Date][wasteEntry.Model].Add(wasteEntry);
                }
                

                double ngPercent = Math.Round(totalNg / totalInspected * 100, 2);
                double scrapPercent = Math.Round(totalScrap / totalInspected * 100, 2);
                double avg = Math.Round(totalInspected / (double)daysOfWork.Count,1);
                string[] operators12h = Load12hOperatorsList();

                DataTable tagTable = new DataTable();
                tagTable.Columns.Add("Data");
                tagTable.Columns.Add("Model");
                tagTable.Columns.Add("Ilość");
                tagTable.Columns.Add("NG");
                tagTable.Columns.Add("Scrap");

                foreach (var dateEntry in operatorperDay)
                {
                    double dayTotal = 0;
                    double dayTotalNg = 0;
                    double dayTotalScrap = 0;
                    foreach (var modelEnry in dateEntry.Value)
                    {
                        double total = modelEnry.Value.Select(q => q.AllQty).Sum();
                        double totNg = modelEnry.Value.Select(q => q.AllNg).Sum();
                        double totScrap = modelEnry.Value.Select(q => q.AllScrap).Sum();
                        dayTotal += total;
                        dayTotalNg += totNg;
                        dayTotalScrap += totScrap;

                        tagTable.Rows.Add("", modelEnry.Key, total, totNg, totScrap);
                    }

                    tagTable.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), "Total", dayTotal, dayTotalNg, dayTotalScrap);
                }
                double h = 8;
                System.Drawing.Color rowClr = System.Drawing.Color.White;
                if (operators12h.Length > 0)
                    if (operators12h.Contains(operatorEntry.Key))
                    {
                        h = 12;
                        rowClr = System.Drawing.Color.LightBlue;
                    }
                

                grid.Rows.Add(operatorEntry.Key, totalInspected, avg, h, Math.Round(avg/h,0), totalNg, ngPercent, totalScrap, scrapPercent);
                grid.Rows[grid.Rows.Count - 1].Cells["Operator"].Tag = tagTable;
                if (h == 12)
                {
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowClr;
                    }
                }
            }

            SMTOperations.autoSizeGridColumns(grid);
            //grid.DataSource = result;
           // grid.Columns.Add(hoursCol);

        }

        public static DataTable checkMstViIfDone(List<excelOperations.order12NC> mstOrders, List<WasteDataStructure> inspectionData)
        {
            DataTable result = new DataTable();
            result.Columns.Add("12NC");
            result.Columns.Add("NrZlecenia");
            result.Columns.Add("Ilość");
            result.Columns.Add("Data przesunięcia");
            result.Columns.Add("Kontrola wzrokowa");
            List<string> ordersInspected = inspectionData.Select(o => o.NumerZlecenia).ToList();
            

            foreach (var mstOrder in mstOrders)
            {
                string date = "";
                string inspectionStatus = "";
                if (ordersInspected.Contains(mstOrder.order))
                    {
                    inspectionStatus = "OK";
                }
                else
                {
                    inspectionStatus = "NIE";

                }
                //Debug.WriteLine(mstOrder.endDate);
                if (mstOrder.endDate > new DateTime(2017, 01, 01))
                {
                    date = mstOrder.endDate.ToString("dd-MM-yyyy");
                }
                

                result.Rows.Add(mstOrder.nc12, mstOrder.order, mstOrder.quantity, date, inspectionStatus);
            }
            return result;
        }

        public static string[] Load12hOperatorsList()
        {
            string[] result = new string[0];
            if (System.IO.File.Exists("operatos12h.txt"))
            {
                result = System.IO.File.ReadAllLines("operatos12h.txt");

            }
            return result;
        }

        public static void Save12hOperators(DataGridView grid)
        {
            List<string> ops = new List<string>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["h/zmiane"].Value.ToString() == "12")
                {
                    ops.Add(row.Cells["Operator"].Value.ToString());
                }
            }
            try
            {
                System.IO.File.WriteAllLines("operatos12h.txt", ops.ToArray());
            }
            catch (Exception ex) { }
        }
    }
}
