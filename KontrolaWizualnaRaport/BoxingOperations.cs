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
    class BoxingOperations
    {
        public static void FillOutBoxingTable(Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> boxingData, DataGridView grid)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();
            Color rowColor = Color.White;

            grid.Columns.Add("Data", "Data");
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

                foreach (var shiftEntry in dateEntry.Value)
                {
                    string date = dateEntry.Key.Date.ToString("yyyy-MM-dd");
                    string shift = shiftEntry.Key.ToString();


                    DataTable shiftTable = new DataTable();
                    shiftTable.Columns.Add("Data");
                    shiftTable.Columns.Add("Zmiana");
                    shiftTable.Columns.Add("Ilosc");
                    shiftTable.Columns.Add("Model");
                    double shiftQty=0;
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

                    grid.Rows.Add(date, shift, shiftQty);
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
