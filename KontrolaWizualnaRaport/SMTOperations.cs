using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class SMTOperations
    {
        public struct LedWasteStruc
        {
            public string lot;
            public string smtLine;
            public string model;
            public int manufacturedModules;
            public int reelsUsed;
            public int ledsPerReel;
            public int requiredRankA;
            public int requiredRankB;
            public int ledLeftA;
            public int ledLeftB;
        }

        public static void FillOutLedWasteTotalByLine(SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>> ledWasteDictionary, DataGridView grid, string model)
        {
            grid.SuspendLayout();
            grid.Rows.Clear();
            List<string> lines = ledWasteDictionary.SelectMany(date => date.Value).SelectMany(shift => shift.Value).Select(l => l.smtLine).Distinct().OrderBy(l => l).ToList();
            
            Dictionary<string, double> producedPerLineA = new Dictionary<string, double>();
            Dictionary<string, double> producedPerLineB = new Dictionary<string, double>();
            Dictionary<string, double> wastePerLineA = new Dictionary<string, double>();
            Dictionary<string, double> wastePerLineB = new Dictionary<string, double>();
            Dictionary<string, DataTable> tagTable = new Dictionary<string, DataTable>();

            DataTable template = new DataTable();
            template.Columns.Add("LOT");
            template.Columns.Add("Model");
            template.Columns.Add("Data");
            template.Columns.Add("Linia");
            template.Columns.Add("Mont.A");
            template.Columns.Add("Odp_A");
            template.Columns.Add("Mont.B");
            template.Columns.Add("Odp_B");

            grid.Columns.Clear();
            grid.Columns.Add("Poz", "");
            foreach (var item in lines)
            {
                grid.Columns.Add(item, item);
                producedPerLineA.Add(item, 0);
                producedPerLineB.Add(item, 0);
                wastePerLineA.Add(item, 0);
                wastePerLineB.Add(item, 0);
                tagTable.Add(item, template.Clone());
                tagTable[item].Rows.Add();
            }

            foreach (var dateEntry in ledWasteDictionary)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lot in shiftEntry.Value)
                    {
                        if (lot.model != model & model != "Wszystkie") continue;
                        if (lot.manufacturedModules < 1) continue;

                        int ledExpectedUsageA = lot.requiredRankA * lot.manufacturedModules;
                        int ledExpectedUsageB = lot.requiredRankB * lot.manufacturedModules;
                        int ledExpectedLeftoversA = lot.reelsUsed / 2 * lot.ledsPerReel - ledExpectedUsageA;
                        int ledExpectedLeftoversB = lot.reelsUsed / 2 * lot.ledsPerReel - ledExpectedUsageB;
                        int droppedA = ledExpectedLeftoversA - lot.ledLeftA;
                        int droppedB = ledExpectedLeftoversB - lot.ledLeftB;

                        producedPerLineA[lot.smtLine] += ledExpectedUsageA;
                        wastePerLineA[lot.smtLine] += droppedA;
                        producedPerLineB[lot.smtLine] += ledExpectedUsageB;
                        wastePerLineB[lot.smtLine] += droppedB;

                        tagTable[lot.smtLine].Rows.Add(lot.lot, lot.model, dateEntry.Key.ToString("dd-MM-yyyy"), lot.smtLine, ledExpectedUsageA, droppedA, ledExpectedUsageB, droppedB);
                    }
                }
            }

            grid.Rows.Add(6);
            foreach (var lineEntry in producedPerLineA)
            {
                grid.Rows[0].Cells[0].Value = "Mont_A";
                grid.Rows[0].Cells[lineEntry.Key].Value = producedPerLineA[lineEntry.Key];
                grid.Rows[0].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                grid.Rows[1].Cells[0].Value = "Odp_A";
                grid.Rows[1].Cells[lineEntry.Key].Value = wastePerLineA[lineEntry.Key];
                grid.Rows[1].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                double wasteA = 0;
                if (producedPerLineA[lineEntry.Key]>0)
                {
                    wasteA = Math.Round(wastePerLineA[lineEntry.Key] / producedPerLineA[lineEntry.Key] * 100, 2);
                }
                grid.Rows[2].Cells[0].Value = "Odp%_A";
                grid.Rows[2].Cells[lineEntry.Key].Value = wasteA + "%";
                grid.Rows[2].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                grid.Rows[3].Cells[0].Value = "Mont_B";
                grid.Rows[3].Cells[lineEntry.Key].Value = producedPerLineB[lineEntry.Key];
                grid.Rows[3].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                grid.Rows[4].Cells[0].Value = "Odp_B";
                grid.Rows[4].Cells[lineEntry.Key].Value = wastePerLineB[lineEntry.Key];
                grid.Rows[4].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];

                double wasteB = 0;
                if (producedPerLineB[lineEntry.Key] > 0)
                {
                    wasteB = Math.Round(wastePerLineB[lineEntry.Key] / producedPerLineB[lineEntry.Key] * 100, 2);
                }
                grid.Rows[5].Cells[0].Value = "Odp%_B";
                grid.Rows[5].Cells[lineEntry.Key].Value = wasteB + "%";
                grid.Rows[5].Cells[lineEntry.Key].Tag = tagTable[lineEntry.Key];
            }
            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static void FillOutLedWasteByModel(SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>> ledWasteDictionary, DataGridView grid, string line)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Model", "Model");
            grid.Columns.Add("Mont_LED", "Mont.LED");
            grid.Columns.Add("Odp_LED", "Odpad LED");
            grid.Columns.Add("Odp", "Odpad");
            grid.Columns.Add("LED", "LED");

            Dictionary<string, double> mountedLed = new Dictionary<string, double>();
            Dictionary<string, DataTable> detailsTag = new Dictionary<string, DataTable>();
            Dictionary<string, double> droppedLed = new Dictionary<string, double>();
            Dictionary<string, double> ledWaste = new Dictionary<string, double>();
            Dictionary<string, string> ledPackage = new Dictionary<string, string>();

            DataTable template = new DataTable();
            template.Columns.Add("LOT");
            template.Columns.Add("Model");
            template.Columns.Add("Data");
            template.Columns.Add("Linia");
            template.Columns.Add("Mont.A");
            template.Columns.Add("Odp_A");
            template.Columns.Add("Mont.B");
            template.Columns.Add("Odp_B");

            foreach (var dateEntry in ledWasteDictionary)
            {
                
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lot in shiftEntry.Value)
                    {
                        if (lot.smtLine != line & line != "Wszystkie") continue;
                        string model = lot.model;
                        string pckg = "";

                        if (lot.ledsPerReel>3000)
                        {
                            pckg = "2835";
                        }
                        else
                        {
                            pckg = "5630";
                        }
                        if(!mountedLed.ContainsKey(model))
                        {
                            mountedLed.Add(model, 0);
                            droppedLed.Add(model, 0);
                            ledWaste.Add(model, 0);
                            detailsTag.Add(model, template.Clone());
                            detailsTag[model].Rows.Add(lot.model + " specA=" + lot.requiredRankA + " specB=" + lot.requiredRankB);
                            ledPackage.Add(model, pckg);

                        }
                        
                        int ledExpectedUsageA = lot.requiredRankA * lot.manufacturedModules;
                        int ledExpectedUsageB = lot.requiredRankB * lot.manufacturedModules;
                        int ledExpectedLeftoversA = lot.reelsUsed / 2 * lot.ledsPerReel - ledExpectedUsageA;
                        int ledExpectedLeftoversB = lot.reelsUsed / 2 * lot.ledsPerReel - ledExpectedUsageB;
                        int droppedA = ledExpectedLeftoversA - lot.ledLeftA;
                        int droppedB = ledExpectedLeftoversB - lot.ledLeftB;

                        detailsTag[model].Rows.Add(lot.lot,lot.model, dateEntry.Key.ToString("dd-MM-yyyy"),lot.smtLine, ledExpectedUsageA, droppedA, ledExpectedUsageB, droppedB);
                        mountedLed[model] += ledExpectedUsageA+ ledExpectedUsageB;
                        droppedLed[model] += droppedA + droppedB;
                        ledWaste[model] = Math.Round(droppedLed[model] / mountedLed[model] * 100, 2);
                    }
                }

                
            }

            foreach (var modelEntry in mountedLed)
            {
                grid.Rows.Add(modelEntry.Key, mountedLed[modelEntry.Key], droppedLed[modelEntry.Key], ledWaste[modelEntry.Key] + "%", ledPackage[modelEntry.Key]);
                foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                {
                    cell.Tag = detailsTag[modelEntry.Key];
                }
            }

            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static void FillOutLedWasteTotalWeekly(SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>> ledWasteDictionary, DataGridView grid)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("MontLED", "MontLED");
            grid.Columns.Add("OdpadLED", "OdpadLED");
            grid.Columns.Add("%", "%");

            Dictionary<string, double> ledMounted = new Dictionary<string, double>();
            Dictionary<string, double> ledDropped = new Dictionary<string, double>();
            Dictionary<string, double> ledWaste = new Dictionary<string, double>();
            double monthMounted = 0;
            double monthDropped = 0;
            double monthwaste = 0;

            string monthName = "";

            foreach (var dateEntry in ledWasteDictionary)
            {
                if (dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture)!=monthName & monthName!="")
                {
                    ledMounted.Add(monthName, monthMounted);
                    ledDropped.Add(monthName, monthDropped);
                    ledWaste.Add(monthName, monthwaste);
                    monthMounted = 0;
                     monthDropped = 0;
                     monthwaste = 0;
                }
                string week = Charting.GetIso8601WeekOfYear(dateEntry.Key).ToString();
                monthName = dateEntry.Key.ToString("MMM", CultureInfo.InvariantCulture);


                if (!ledMounted.ContainsKey(week))
                {
                    ledMounted.Add(week, 0);
                    ledDropped.Add(week, 0);
                    ledWaste.Add(week, 0);
                }
                foreach (var shiftEntry in dateEntry.Value)
                {
                    foreach (var lotData in shiftEntry.Value)
                    {
                        int ledExpectedUsageA = lotData.requiredRankA * lotData.manufacturedModules;
                        int ledExpectedUsageB = lotData.requiredRankB * lotData.manufacturedModules;
                        int ledExpectedLeftoversA = lotData.reelsUsed / 2 * lotData.ledsPerReel - ledExpectedUsageA;
                        int ledExpectedLeftoversB = lotData.reelsUsed / 2 * lotData.ledsPerReel - ledExpectedUsageB;
                        int droppedA = ledExpectedLeftoversA - lotData.ledLeftA;
                        int droppedB = ledExpectedLeftoversB - lotData.ledLeftB;

                        //if (droppedA + droppedB < 0) continue;

                        ledMounted[week] += ledExpectedUsageA + ledExpectedUsageB;
                        ledDropped[week] += droppedA + droppedB;
                        ledWaste[week] = Math.Round(ledDropped[week] / ledMounted[week] * 100, 2);
                        monthMounted += ledExpectedUsageA + ledExpectedUsageB;
                        monthDropped += droppedA + droppedB;
                        monthwaste = Math.Round(ledDropped[week] / ledMounted[week] * 100, 2);
                    }
                }

            }
            foreach (var weekEntry in ledMounted)
            {
                grid.Rows.Add(weekEntry.Key, ledMounted[weekEntry.Key], ledDropped[weekEntry.Key], ledWaste[weekEntry.Key]);
            }
            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static void FillOutDailyLedWaste(SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>> ledWasteDictionary, DataGridView grid)
        {
            grid.SuspendLayout();
            grid.Columns.Clear();
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Zm", "Zm");
            grid.Columns.Add("SMT1", "SMT1");
            grid.Columns.Add("SMT2", "SMT2");
            grid.Columns.Add("SMT3", "SMT3");
            grid.Columns.Add("SMT5", "SMT5");
            grid.Columns.Add("SMT6", "SMT6");
            grid.Columns.Add("SMT7", "SMT7");
            grid.Columns.Add("SMT8", "SMT8");

            foreach (var dateEntry in ledWasteDictionary)
            {
                foreach (var shiftEntry in dateEntry.Value)
                {
                    Dictionary<string, double> ledDroppedPerLine = new Dictionary<string, double>();
                    Dictionary<string, double> ledUsedPerLine = new Dictionary<string, double>();
                    Dictionary<string, string> ledWastePerLine = new Dictionary<string, string>();

                    ledDroppedPerLine.Add("SMT1", 0);
                    ledDroppedPerLine.Add("SMT2", 0);
                    ledDroppedPerLine.Add("SMT3", 0);
                    ledDroppedPerLine.Add("SMT5", 0);
                    ledDroppedPerLine.Add("SMT6", 0);
                    ledDroppedPerLine.Add("SMT7", 0);
                    ledDroppedPerLine.Add("SMT8", 0);

                    ledUsedPerLine.Add("SMT1", 0);
                    ledUsedPerLine.Add("SMT2", 0);
                    ledUsedPerLine.Add("SMT3", 0);
                    ledUsedPerLine.Add("SMT5", 0);
                    ledUsedPerLine.Add("SMT6", 0);
                    ledUsedPerLine.Add("SMT7", 0);
                    ledUsedPerLine.Add("SMT8", 0);

                    ledWastePerLine.Add("SMT1", "");
                    ledWastePerLine.Add("SMT2", "");
                    ledWastePerLine.Add("SMT3", "");
                    ledWastePerLine.Add("SMT5", "");
                    ledWastePerLine.Add("SMT6", "");
                    ledWastePerLine.Add("SMT7", "");
                    ledWastePerLine.Add("SMT8", "");

                    foreach (var lotData in shiftEntry.Value)
                    {
                        int ledExpectedUsageA = lotData.requiredRankA * lotData.manufacturedModules;
                        int ledExpectedUsageB = lotData.requiredRankB * lotData.manufacturedModules;
                        int ledExpectedLeftoversA = lotData.reelsUsed/2 * lotData.ledsPerReel - ledExpectedUsageA;
                        int ledExpectedLeftoversB = lotData.reelsUsed/2 * lotData.ledsPerReel - ledExpectedUsageB;
                        int droppedA = ledExpectedLeftoversA - lotData.ledLeftA;
                        int droppedB = ledExpectedLeftoversB - lotData.ledLeftB;

                        if (droppedA + droppedB < 0) continue;

                        ledUsedPerLine[lotData.smtLine] += ledExpectedUsageA + ledExpectedUsageB;
                        ledDroppedPerLine[lotData.smtLine] += droppedA + droppedB;
                    }

                    foreach (var lineEntry in ledUsedPerLine)
                    {
                        if (ledUsedPerLine[lineEntry.Key] > 0)
                        {
                            ledWastePerLine[lineEntry.Key] = Math.Round(ledDroppedPerLine[lineEntry.Key] / ledUsedPerLine[lineEntry.Key] * 100, 2).ToString()+"%";
                        }
                        else
                        {
                            ledWastePerLine[lineEntry.Key] = "";
                        }
                    }

                    

                    grid.Rows.Add(dateEntry.Key.ToString("dd-MM-yyyy"), shiftEntry.Key, ledWastePerLine["SMT1"], ledWastePerLine["SMT2"] , ledWastePerLine["SMT3"], ledWastePerLine["SMT5"] , ledWastePerLine["SMT6"] , ledWastePerLine["SMT7"], ledWastePerLine["SMT8"]);
                }
            }
            autoSizeGridColumns(grid);
            grid.ResumeLayout();
        }

        public static SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>> ledWasteDictionary(SortedDictionary<DateTime, SortedDictionary<int, DataTable>> inputSmtData, Dictionary<string, MesModels> mesModels)
        {
            SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>> result = new SortedDictionary<DateTime, SortedDictionary<int, List<LedWasteStruc>>>();

            foreach (var dateEntry in inputSmtData)
            {
                if (!result.ContainsKey(dateEntry.Key))
                {
                    result.Add(dateEntry.Key, new SortedDictionary<int, List<LedWasteStruc>>());
                }
                foreach (var shiftEntry in dateEntry.Value)
                {
                    if (!result[dateEntry.Key].ContainsKey(shiftEntry.Key))
                    {
                        result[dateEntry.Key].Add(shiftEntry.Key, new List<LedWasteStruc>());
                    }
                    foreach (DataRow row in shiftEntry.Value.Rows)
                    {
                        //107577:2OPF00050A:0|107658:2OPF00050A:0#107580:2OPF00050A:27|107657:2OPF00050A:23
                        string lot = row["NrZlecenia"].ToString();
                        string model = row["Model"].ToString();
                        int requiredRankA = mesModels["LLFML"+model].LedAQty;
                        int requiredRankB = mesModels["LLFML" + model].LedBQty;
                        string[] ledDropped = row["KoncowkiLED"].ToString().Split('#');
                        int reelsUsed = ledDropped.Length * 2;
                        int ledALeftTotal = 0;
                        int ledBLeftTotal = 0;
                        int ledPerReel = 0;
                        int manufacturedModules = int.Parse(row["IloscWykonana"].ToString());
                        string smtLine = row["LiniaSMT"].ToString();
                        

                        foreach (var item in ledDropped) 
                        {
                            string[] ranks = item.Split('|');
                            string[] rankA = ranks[0].ToString().Split(':');
                            string[] rankB = ranks[1].ToString().Split(':');
                            int leftA = int.Parse(rankA[2]);
                            int leftB = int.Parse(rankB[2]);
                            string ledId = rankA[1];
                            if (ledId.Length>10)
                            {
                                ledPerReel = 3000;
                            }
                            else
                            {
                                ledPerReel = 3500;
                            }
                            ledALeftTotal += leftA;
                            ledBLeftTotal += leftB;
                        }

                        if (ledPerReel * reelsUsed / 2 - requiredRankA * manufacturedModules < ledALeftTotal || ledPerReel * reelsUsed / 2 - requiredRankB * manufacturedModules < ledBLeftTotal) 
                        {
                            continue;
                        }

                        LedWasteStruc newItem = new LedWasteStruc();
                        newItem.lot = lot;
                        newItem.requiredRankA = requiredRankA;
                        newItem.requiredRankB = requiredRankB;
                        newItem.ledLeftA = ledALeftTotal;
                        newItem.ledLeftB = ledBLeftTotal;
                        newItem.ledsPerReel = ledPerReel;
                        newItem.manufacturedModules = manufacturedModules;
                        newItem.smtLine = smtLine;
                        newItem.reelsUsed = reelsUsed;
                        newItem.model = model;
                        result[dateEntry.Key][shiftEntry.Key].Add(newItem);
                    }
                }
            }

            return result;
        }


        public static SortedDictionary<DateTime, SortedDictionary<int, DataTable>> sortTableByDayAndShift(DataTable sqlTable,string dateColumnName)
        {
            //DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc
            SortedDictionary<DateTime, SortedDictionary<int, DataTable>> summaryDic = new SortedDictionary<DateTime, SortedDictionary<int, DataTable>>();

            foreach (DataRow row in sqlTable.Rows)
            {
                string dateString = row[dateColumnName].ToString();
                if (dateString == "") continue;
                //DateTime endDate = DateTime.ParseExact(dateString, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.Parse(dateString);
                dateShiftNo endDateShiftInfo = whatDayShiftIsit(endDate);

                if (!summaryDic.ContainsKey(endDateShiftInfo.date.Date))
                {
                    summaryDic.Add(endDateShiftInfo.date.Date, new SortedDictionary<int, DataTable>());
                }
                if (!summaryDic[endDateShiftInfo.date.Date].ContainsKey(endDateShiftInfo.shift))
                {
                    summaryDic[endDateShiftInfo.date.Date].Add(endDateShiftInfo.shift, new DataTable());
                    summaryDic[endDateShiftInfo.date.Date][endDateShiftInfo.shift] = sqlTable.Clone();
                }
                summaryDic[endDateShiftInfo.date.Date][endDateShiftInfo.shift].Rows.Add(row.ItemArray);
            }

            return summaryDic;
        }

        public struct dateShiftNo
        {
            public DateTime date;
            public int shift;
        }

        ///<summary>
        ///<para>returns shift number and shift start date and time</para>
        ///</summary>
        public static dateShiftNo whatDayShiftIsit(DateTime inputDate)
        {
            int hourNow = inputDate.Hour;
            DateTime resultDate = new DateTime();
            int resultShift = 0;

            if (hourNow < 6)
            {
                resultDate = new DateTime(inputDate.Date.AddDays(-1).Year, inputDate.Date.AddDays(-1).Month, inputDate.Date.AddDays(-1).Day , 22, 0, 0);
                resultShift = 3;
            }

            else if (hourNow < 14)
            {
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day, 6, 0, 0);
                resultShift = 1;
            }

            else if (hourNow < 22)
            {
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day, 14, 0, 0);
                resultShift = 2;
            }

            else
            {
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day, 22, 0, 0);
                resultShift = 3;
            }

            dateShiftNo result = new dateShiftNo();
            result.date = resultDate;
            result.shift = resultShift;

            return result;
        }

        public static void shiftSummaryDataSource(SortedDictionary<DateTime, SortedDictionary<int, DataTable>> sourceDic, DataGridView grid)
        {
            DataTable result = new DataTable();
            grid.Rows.Clear();
            grid.Columns.Clear();
            grid.Columns.Add("Dzien", "Dzien");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("SMT1", "SMT1");
            grid.Columns.Add("SMT2", "SMT2");
            grid.Columns.Add("SMT3", "SMT3");
            grid.Columns.Add("SMT5", "SMT5");
            grid.Columns.Add("SMT6", "SMT6");
            grid.Columns.Add("SMT7", "SMT7");
            grid.Columns.Add("SMT8", "SMT8");
            System.Drawing.Color rowColor = System.Drawing.Color.LightBlue;

            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (var dayEntry in sourceDic)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }

                foreach (var shiftEntry in dayEntry.Value)
                {
                    Dictionary<string, double> qtyPerLine = new Dictionary<string, double>();
                    Dictionary<string, DataTable> detailPerLine = new Dictionary<string, DataTable>();

                    foreach (DataRow row in shiftEntry.Value.Rows)
                    {
                        string smtLine = row["LiniaSMT"].ToString();
                        if (!qtyPerLine.ContainsKey(smtLine))
                        {
                            qtyPerLine.Add(smtLine, 0);
                            detailPerLine.Add(smtLine, new DataTable());
                            detailPerLine[smtLine] = shiftEntry.Value.Clone();
                        }
                        double qty = double.Parse(row["IloscWykonana"].ToString());
                        qtyPerLine[smtLine] += qty;
                        detailPerLine[smtLine].Rows.Add(row.ItemArray);
                    }

                    double smt1 = 0;
                    double smt2 = 0;
                    double smt3 = 0;
                    double smt5 = 0;
                    double smt6 = 0;
                    double smt7 = 0;
                    double smt8 = 0;

                    foreach (var lineEntry in qtyPerLine)
                    {
                        qtyPerLine.TryGetValue("SMT1", out smt1);
                        qtyPerLine.TryGetValue("SMT2", out smt2);
                        qtyPerLine.TryGetValue("SMT3", out smt3);
                        qtyPerLine.TryGetValue("SMT5", out smt5);
                        qtyPerLine.TryGetValue("SMT6", out smt6);
                        qtyPerLine.TryGetValue("SMT7", out smt7);
                        qtyPerLine.TryGetValue("SMT8", out smt8);
                    }
                    
                    grid.Rows.Add(dayEntry.Key.ToShortDateString(), shiftEntry.Key.ToString(), smt1, smt2, smt3, smt5, smt6, smt7, smt8);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count-1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        DataTable dt;
                        if (detailPerLine.TryGetValue(cell.OwningColumn.Name, out dt))
                            {
                            cell.Tag = dt;
                        }
                        
                    }
                }
            }
            autoSizeGridColumns(grid);
            if (grid.Rows.Count > 0)
            {
                grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
            }
        }

        public static void autoSizeGridColumns(DataGridView grid)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        public static Dictionary<string, Dictionary<string, List<durationQuantity>>> smtQtyPerModelPerLine (DataTable smtRecords, bool showAllModels)
        {
            Dictionary<string, Dictionary<string, List<durationQuantity>>> result = new Dictionary<string, Dictionary<string, List<durationQuantity>>>();
            foreach (DataRow row in smtRecords.Rows)
            {
                string model = row["Model"].ToString();
                string modelShort = model.Substring(0, 6) + "X0X" + model.Substring(9, 1);
                string line = row["LiniaSMT"].ToString();
                double qty = 0;
                if (!double.TryParse(row["IloscWykonana"].ToString(), out qty)) continue;
                //DataCzasStart,DataCzasKoniec
                DateTime dateStart = new DateTime();
                DateTime dateEnd = new DateTime();
                if (!DateTime.TryParse(row["DataCzasStart"].ToString(), out dateStart) || !DateTime.TryParse(row["DataCzasKoniec"].ToString(), out dateEnd)) continue;
                var lotDuration = (dateEnd - dateStart).TotalHours;

                if (lotDuration < 0.25) continue;

                //Debug.WriteLine(lotDuration);
                if (!result.ContainsKey(model) & showAllModels)
                {
                    result.Add(model, new Dictionary<string, List<durationQuantity>>());
                }
                if (!result.ContainsKey(modelShort))
                {
                    result.Add(modelShort, new Dictionary<string, List<durationQuantity>>());
                }
                if (showAllModels)
                {
                    if (!result[model].ContainsKey(line))
                    {
                        result[model].Add(line, new List<durationQuantity>());
                    }
                }
                if (!result[modelShort].ContainsKey(line))
                {
                    result[modelShort].Add(line, new List<durationQuantity>());
                }

                durationQuantity newItem = new durationQuantity();
                newItem.duration = lotDuration;
                newItem.quantity = qty;
                newItem.start = dateStart;
                newItem.end = dateEnd;
                newItem.lot = row["NrZlecenia"].ToString();

                if (showAllModels)
                {
                    result[model][line].Add(newItem);
                }
                result[modelShort][line].Add(newItem);
            }

            return result;
        }

        public struct durationQuantity
        {
            public double duration;
            public double quantity;
            public string lot;
            public DateTime start;
            public DateTime end;
        }

        public static DataTable MakeTableForModelEfficiency(Dictionary<string, Dictionary<string, List<durationQuantity>>> inputData,string model, bool perShift)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Linia");
            result.Columns.Add("Ilość całkowita");
            result.Columns.Add("Średnia/h");
            result.Columns.Add("Min/h");
            result.Columns.Add("Max/h");
            double frequency = 1;
            if(!perShift)
            {
                frequency = 8;
            }
            

            foreach (var modelEntry in inputData)
            {
                if (modelEntry.Key != model) continue;
                foreach (var lineEntry in modelEntry.Value)
                {
                    List<double> checkList = new List<double>();
                    foreach (var lot in lineEntry.Value)
                    {
                        checkList.Add(lot.quantity / lot.duration * frequency);
                        Debug.WriteLine(lot.quantity + "szt. " + lot.start.ToShortTimeString() + "-" + lot.end.ToShortTimeString() + " " + lot.duration * 60 + "min. " + 8*lot.quantity / lot.duration + "szt./zm ");
                    }

                    checkList.Sort();
                    double totalQty = lineEntry.Value.Select(q => q.quantity).Sum() * frequency;
                    double min = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duration).Min(),0) * frequency;
                    double max = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duration).Max(), 0) * frequency;
                    double avg = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duration).Average(), 0) * frequency;
                    double median = Math.Round(checkList[checkList.Count / 2], 0) ;
                    result.Rows.Add(lineEntry.Key, totalQty, median ,min, max);
                }
            }
            return result;
        }
    }
}
