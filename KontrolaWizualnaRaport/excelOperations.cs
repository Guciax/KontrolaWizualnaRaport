using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KontrolaWizualnaRaport
{
    class excelOperations
    {
        public struct order12NC{
            public string nc12;
            public string order;
            public string quantity;
        }

        public static List<order12NC> loadExcel(ref Dictionary<string, string> lotModelDictionary)
        {
            List<order12NC> result = new List<order12NC>();
            string FilePath = @"Y:\Manufacturing_Center\Manufacturing HID EM\weinne\woto\elektronika\ZLECENIA MST\2018\zlecenia MST.xlsx";

            if (File.Exists(FilePath))
            {
                var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var pck = new OfficeOpenXml.ExcelPackage();
                try
                {
                    pck = new OfficeOpenXml.ExcelPackage(fs);
                }
                catch (Exception e) { MessageBox.Show(e.Message); }

                if (pck.Workbook.Worksheets.Count != 0)
                {
                    
                    foreach (OfficeOpenXml.ExcelWorksheet worksheet in pck.Workbook.Worksheets)
                    {
                        int orderColIndex = 0;
                        int nc12ColIndex = 0;
                        int qtyColIndex = 0;
                        int firstRowData = 0;
                        for (int row = 1; row < 11; row++)
                        {
                            for (int col = 1; col < worksheet.Dimension.End.Column; col++)
                            {
                                if (worksheet.Cells[row, col].Value != null)
                                {
                                    if (worksheet.Cells[row, col].Value.ToString().Trim().ToUpper().Replace(" ", "") == "NRZLECENIA")
                                    {
                                        orderColIndex = col;
                                    }
                                    if (worksheet.Cells[row, col].Value.ToString().Trim().ToUpper().Replace(" ", "") == "12NC")
                                    {
                                        nc12ColIndex = col;
                                    }
                                    if (worksheet.Cells[row, col].Value.ToString().Trim().ToUpper().Replace(" ", "") == "ILOŚĆ")
                                    {
                                        qtyColIndex = col;
                                    }
                                    if (orderColIndex > 0 & nc12ColIndex > 0 & qtyColIndex > 0) 
                                    {
                                        firstRowData = row + 1;
                                        break;
                                    }
                                }
                            }
                            if (orderColIndex > 0) break;
                        }

                        for (int row = firstRowData; row < worksheet.Dimension.End.Row; row++)
                        {
                            if (worksheet.Cells[row, nc12ColIndex].Value != null)
                            {
                                string nc12 = worksheet.Cells[row, nc12ColIndex].Value.ToString().Replace(" ", "").Trim();
                                string orderNo = worksheet.Cells[row, orderColIndex].Value.ToString().Replace(" ", "").Trim();
                                string qty = worksheet.Cells[row, qtyColIndex].Value.ToString().Replace(" ", "").Trim();
                                order12NC newItem = new order12NC();
                                newItem.order = orderNo;
                                newItem.nc12 = nc12;
                                newItem.quantity = qty;
                                result.Add(newItem);
                            }
                        }
                    }
               }
            }

            foreach (var item in result)
            {
                if (lotModelDictionary.ContainsKey(item.order)) continue;
                lotModelDictionary.Add(item.order, item.nc12);
            }

            return result;
        }
    }
}
