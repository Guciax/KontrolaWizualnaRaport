using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static DataTable ngRatePerOperator(List<WasteDataStructure> inspectionData, DateTime startDate, DateTime endDate)
        {
            DataTable result = new DataTable();
            Dictionary<string, List<WasteDataStructure>> inspectionDataPerOperator = inspectionData.GroupBy(op => op.Oper).ToDictionary(op => op.Key, op => op.ToList());

            result.Columns.Add("Operator");
            result.Columns.Add("Sprawdzone", typeof (double));
            result.Columns.Add("NG", typeof(double));
            result.Columns.Add("NG%", typeof(double));
            result.Columns.Add("Scrap", typeof(double));
            result.Columns.Add("Scrap%", typeof(double));

            foreach (var operatorEntry in inspectionDataPerOperator)
            {
                //double totalInspected = operatorEntry.Value.Select(t => t.AllQty).Sum();
                //double totalNg = operatorEntry.Value.Select(t => t.AllNg).Sum();
                //double ngPercent = Math.Round(totalNg / totalInspected * 100, 2);
                //double totalScrap = operatorEntry.Value.Select(t => t.AllScrap).Sum();
                //double scrapPercent = Math.Round(totalScrap / totalInspected * 100, 2);

                double totalInspected = 0;
                double totalNg = 0;
                double totalScrap = 0;


                foreach (var wasteEntry in operatorEntry.Value)
                {
                    if (wasteEntry.FixedDateTime.Date < startDate.Date || wasteEntry.FixedDateTime.Date > endDate.Date) continue;
                    totalInspected += wasteEntry.AllQty;
                    totalNg += wasteEntry.AllNg;
                    totalScrap += wasteEntry.AllScrap;
                }

                double ngPercent = Math.Round(totalNg / totalInspected * 100, 2);
                double scrapPercent = Math.Round(totalScrap / totalInspected * 100, 2);

                result.Rows.Add(operatorEntry.Key, totalInspected, totalNg, ngPercent, totalScrap, scrapPercent);
            }

            return result;
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
    }
}
