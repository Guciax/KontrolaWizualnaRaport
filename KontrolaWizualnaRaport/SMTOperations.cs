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
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }

        public static void autoSizeGridColumns(DataGridView grid)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        public static Dictionary<string, Dictionary<string, List<durationQuantity>>> smtQtyPerModelPerLine (DataTable smtRecords)
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
                if (lotDuration < 0.15) continue;
                //Debug.WriteLine(lotDuration);
                if (!result.ContainsKey(model))
                {
                    result.Add(model, new Dictionary<string, List<durationQuantity>>());
                }
                if (!result.ContainsKey(modelShort))
                {
                    result.Add(modelShort, new Dictionary<string, List<durationQuantity>>());
                }
                if (!result[model].ContainsKey(line))
                {
                    result[model].Add(line, new List<durationQuantity>());
                }
                if (!result[modelShort].ContainsKey(line))
                {
                    result[modelShort].Add(line, new List<durationQuantity>());
                }

                durationQuantity newItem = new durationQuantity();
                newItem.duration = lotDuration;
                newItem.quantity = qty;

                result[model][line].Add(newItem);
                result[modelShort][line].Add(newItem);
            }

            return result;
        }

        public struct durationQuantity
        {
            public double duration;
            public double quantity;
        }

        public static DataTable MakeTableForModel(Dictionary<string, Dictionary<string, List<durationQuantity>>> inputData,string model)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Linia");
            result.Columns.Add("Ilość całkowita");
            result.Columns.Add("Średnia/h");
            result.Columns.Add("Min/h");
            result.Columns.Add("Max/h");
            

            foreach (var modelEntry in inputData)
            {
                if (modelEntry.Key != model) continue;
                foreach (var lineEntry in modelEntry.Value)
                {
                    
                    double totalQty = lineEntry.Value.Select(q => q.quantity).Sum();
                    double min = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duration).Min(),0);
                    double max = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duration).Max(), 0);
                    double avg = Math.Round(lineEntry.Value.Select(q => q.quantity / q.duration).Average(), 0);
                    double median = Math.Round(lineEntry.Value[Convert.ToInt16(Math.Truncate((decimal)(lineEntry.Value.Count / 2)))].quantity / lineEntry.Value[Convert.ToInt16(Math.Truncate((decimal)(lineEntry.Value.Count / 2)))].duration, 0);

                    result.Rows.Add(lineEntry.Key, totalQty, median ,min, max);

                }
            }

            return result;
        }
    }
}
