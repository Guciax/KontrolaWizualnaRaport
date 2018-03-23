using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class SMTOperations
    {
        public static Dictionary<DateTime, Dictionary<int, DataTable>> sortTableByDayAndShift(DataTable sqlTable)
        {
            //DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc
            Dictionary<DateTime, Dictionary<int, DataTable>> summaryDic = new Dictionary<DateTime, Dictionary<int, DataTable>>();

            foreach (DataRow row in sqlTable.Rows)
            {
                string dateString = row["DataCzasKoniec"].ToString();
                //DateTime endDate = DateTime.ParseExact(dateString, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.Parse(dateString);
                dateShiftNo endDateShiftInfo = whatDayShiftIsit(endDate);

                if (!summaryDic.ContainsKey(endDateShiftInfo.date.Date))
                {
                    summaryDic.Add(endDateShiftInfo.date.Date, new Dictionary<int, DataTable>());
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
                resultDate = new DateTime(inputDate.Date.Year, inputDate.Date.Month, inputDate.Date.Day - 1, 22, 0, 0);
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

        public static void shiftSummaryDataSource(Dictionary<DateTime, Dictionary<int, DataTable>> sourceDic, DataGridView grid)
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
        }

        public static void autoSizeGridColumns(DataGridView grid)
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

    }
}
