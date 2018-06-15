using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    class dataLoader
    {
        public static List<WasteDataStructure> LoadData(DataTable inputTable, Dictionary<string, string> lotToSmtLine, Dictionary<string, string> lotToModel)
        {
            List<WasteDataStructure> result = new List<WasteDataStructure>();

            foreach (DataRow row in inputTable.Rows)
            {
                int id;
                DateTime fixedDateTime;
                DateTime realDateTime;
                int shiftNumber;
                string oper;
                int goodQty;
                int allQty=0;
                string numerZlecenia;
                int allNg = 0;
                int allScrap = 0;

                Dictionary<string, int> wastePerReason = new Dictionary<string, int>();
                
                id = int.Parse(row["Id"].ToString());
                realDateTime = ParseExact(row["Data_czas"].ToString());
                fixedDateTime = FixedShiftDate(realDateTime);
                shiftNumber = DateToShiftNumber(realDateTime);
                oper = row["Operator"].ToString();
                goodQty = int.Parse(row["iloscDobrych"].ToString());
                allQty = goodQty;
                string model = "???";
                

                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                string digitsLot = rgx.Replace(row["numerZlecenia"].ToString(), "");
                numerZlecenia = digitsLot.Trim();
                if (lotToModel.ContainsKey(numerZlecenia))
                {
                    model = lotToModel[numerZlecenia];
                }

                    for (int c = 0; c < inputTable.Columns.Count; c++)
                {
                    string wasteReason = inputTable.Columns[c].ColumnName;
                    if (wasteReason.Contains("ng"))
                    {
                        if (!wastePerReason.ContainsKey(wasteReason))
                        {
                            wastePerReason.Add(wasteReason, 0);
                        }

                        int wasteQty = int.Parse(row[c].ToString());
                        wastePerReason[wasteReason] = wasteQty;
                        allNg += wasteQty;

                    } else if (wasteReason.Contains("scrap"))
                    {
                        if (!wastePerReason.ContainsKey(wasteReason))
                        {
                            wastePerReason.Add(wasteReason, 0);
                        }

                        int wasteQty = int.Parse(row[c].ToString());
                        wastePerReason[wasteReason] = wasteQty;
                        allScrap += wasteQty;
                    }
                }
                string smtLine = "";
                if (!lotToSmtLine.TryGetValue(digitsLot, out smtLine))
                {
                    smtLine = "";
                }

                WasteDataStructure recordToAdd = new WasteDataStructure(id, fixedDateTime, realDateTime, shiftNumber, oper, goodQty, allQty,allNg,allScrap, numerZlecenia, model, wastePerReason, smtLine);
                result.Add(recordToAdd);
            }

            return result;
        }

        public static DateTime ParseExact(string date)
        {
            try
            {
                if (date.Contains("-"))
                    return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                if (date.Contains(@"/"))
                    return DateTime.ParseExact(date, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                else
                    return DateTime.ParseExact(date, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
            }
            catch (Exception e)
            {
                return new DateTime(1900, 1, 1);
            }
        }
        public static DateTime FixedShiftDate(DateTime inputDate)
        {
            if (inputDate.Hour < 6)
            {
                return inputDate.AddDays(-1);

            }
            else return inputDate;
        }

        public static int DateToShiftNumber(DateTime inputDate)
        {
            if (inputDate.Hour >= 22)
            {
                return  3;
            }
            if (inputDate.Hour >= 14) return 2;
            if (inputDate.Hour >= 6) return 1;

            return 0;
        }
    }

}
