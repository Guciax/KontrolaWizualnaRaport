using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class SplittingOperations
    {
        public static void FillGrid(DataTable lotTable, DataGridView grid)
        {
            DataTable dt = lotTable.Copy();
            dt.Columns.Remove("LiniaProdukcyjna");
            SortedDictionary<DateTime, SortedDictionary<int, DataTable>> tablesPerDayPerShift = SMTOperations.sortTableByDayAndShift(dt, "Data_Konca_Zlecenia");

            System.Drawing.Color rowColor = System.Drawing.Color.LightBlue;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (var dayEntry in tablesPerDayPerShift)
            {
                if (rowColor == System.Drawing.Color.LightBlue)
                {
                    rowColor = System.Drawing.Color.White;
                }
                else
                {
                    rowColor = System.Drawing.Color.LightBlue;
                }
                var week = dateTools.GetIso8601WeekOfYear(dayEntry.Key);
                foreach (var shiftEntry in dayEntry.Value)
                {
                    double qty = 0;
                    foreach (DataRow row in shiftEntry.Value.Rows)
                    {
                        qty += double.Parse(row["Ilosc_wyrobu_zlecona"].ToString());
                    }

                    grid.Rows.Add(dayEntry.Key.ToShortDateString(),week, shiftEntry.Key.ToString(), qty);
                    foreach (DataGridViewCell cell in grid.Rows[grid.Rows.Count - 1].Cells)
                    {
                        cell.Style.BackColor = rowColor;
                        if (cell.OwningColumn.HeaderText == "Ilość")
                        {
                            cell.Tag = shiftEntry.Value;
                        }
                    }
                }
            }
            grid.FirstDisplayedScrollingRowIndex = grid.RowCount - 1;
        }
    }
}
