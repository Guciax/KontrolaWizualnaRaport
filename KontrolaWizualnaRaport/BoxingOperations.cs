using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class BoxingOperations
    {
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static void FillOutBoxingLedQty(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> boxingData, Dictionary<string, MesModels> mesModels, DataGridView grid)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Tydz", "Tydz");
            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("IloscWszystkie", "IloscWszystkie");
            grid.Columns.Add("IloscKwadrat", "IloscKwadrat");

            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            foreach (var dateEntry in boxingData)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }

                foreach (var shiftEntry in dateEntry.Value)
                {
                    Int32 ledCountAll = 0;
                    Int32 ledCountSquare = 0;

                    foreach (var modelEntry in shiftEntry.Value)
                    {
                        int ledPerModel = mesModels[modelEntry.Key].LedSumQty;
                        string type = mesModels[modelEntry.Key].Type;
                        ledCountAll += ledPerModel * modelEntry.Value.Rows.Count;
                        if (type == "square")
                        {
                            ledCountSquare += ledPerModel * modelEntry.Value.Rows.Count;
                        }
                    }

                    grid.Rows.Add(GetIso8601WeekOfYear(dateEntry.Key), dateEntry.Key.ToString("dd-MM-yyyy"), shiftEntry.Key, ledCountAll, ledCountSquare);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                    }
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }

        public static void FillOutBoxingTable(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> boxingData, DataGridView grid)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Data", "Data");
            grid.Columns.Add("Tydz", "tydz");
            grid.Columns.Add("Zmiana", "Zmiana");
            grid.Columns.Add("Ilosc", "Ilosc");


            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            foreach (var dateEntry in boxingData)
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


                    DataTable shiftTable = new DataTable();
                    shiftTable.Columns.Add("Data");
                    shiftTable.Columns.Add("Zmiana");
                    shiftTable.Columns.Add("Ilosc");
                    shiftTable.Columns.Add("Model");
                    double shiftQty = 0;
                    Dictionary<string, double> qtyPerModel = new Dictionary<string, double>();
                    foreach (var modelEntry in shiftEntry.Value)
                    {
                        if (!qtyPerModel.ContainsKey(modelEntry.Key))
                        {
                            qtyPerModel.Add(modelEntry.Key, 0);
                        }
                        qtyPerModel[modelEntry.Key] = modelEntry.Value.Rows.Count;
                    }

                    foreach (var modelEntry in qtyPerModel)
                    {
                        shiftTable.Rows.Add(dateEntry.Key, shiftEntry.Key, modelEntry.Value, modelEntry.Key);
                        shiftQty += modelEntry.Value;
                    }

                    grid.Rows.Add(date, week, shift, shiftQty);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.ColumnIndex > 1)
                        {
                            string tester = cell.OwningColumn.Name;
                            cell.Tag = shiftTable;
                        }
                    }
                }
            }
            SMTOperations.autoSizeGridColumns(grid);
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}
