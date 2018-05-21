using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KontrolaWizualnaRaport.SMTOperations;

namespace KontrolaWizualnaRaport
{

    class SQLoperations

    {
        private readonly Form1 form;
        private readonly TextBox console;

        public SQLoperations(Form1 form, TextBox console)
        {
            this.form = form;
            this.console = console;
        }

        public static DataTable DownloadVisInspFromSQL(int daysAgo)
        {
            DateTime tillDate = System.DateTime.Now.AddDays(daysAgo * (-1));
            HashSet<string> result = new HashSet<string>();
            DataTable tabletoFill = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = @"SELECT Id,Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy where Data_czas>@dataCzas;";
            //@"SELECT Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy WHERE Data_czas > '" + DateTime.Now.AddDays(-90).ToShortDateString() + "';";
            command.Parameters.AddWithValue("@dataCzas", tillDate);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            //try
            {
                adapter.Fill(tabletoFill);
            }
           // catch (Exception e)
            {
                //console.Text+="OP LOADER: " + e.Message + Environment.NewLine;
            }
            return tabletoFill;
        }

        public static DataTable lotTable()
        {
            DataTable result = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT Nr_Zlecenia_Produkcyjnego,NC12_wyrobu,Ilosc_wyrobu_zlecona,LiniaProdukcyjna,DataCzasWydruku,Data_Konca_Zlecenia FROM tb_Zlecenia_produkcyjne;";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static Dictionary<string,string>[] LotList()
        {
            Dictionary<string, string> result1 = new Dictionary<string, string>();
            Dictionary<string, string> result2 = new Dictionary<string, string>();
            Dictionary<string, string> result3 = new Dictionary<string, string>();

            DataTable sqlTable = new DataTable();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT Nr_Zlecenia_Produkcyjnego,NC12_wyrobu,Ilosc_wyrobu_zlecona,LiniaProdukcyjna,DataCzasWydruku FROM tb_Zlecenia_produkcyjne order by DataCzasWydruku;";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                if (result1.ContainsKey(row["Nr_Zlecenia_Produkcyjnego"].ToString())) continue;
                result1.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["NC12_wyrobu"].ToString().Replace("LLFML",""));
                result2.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["Ilosc_wyrobu_zlecona"].ToString());
                result3.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["LiniaProdukcyjna"].ToString());
            }
            Dictionary<string, string>[] result = new Dictionary<string, string>[] { result1, result2, result3 };
            return result;
        }

        public static Dictionary<DateTime, SortedDictionary<int,Dictionary<string, DataTable>>> GetBoxing(int daysAgo)
        {
            DataTable sqlTable = new DataTable();
            DateTime untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6);

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT Boxing_Date,NC12_wyrobu,Wysylki_Nr FROM v_WyrobLG_opakowanie_all WHERE Boxing_Date>@until order by Boxing_Date;");
            command.Parameters.AddWithValue("@until", untilDay);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            sqlTable.Columns["Boxing_Date"].ColumnName = "Data";
            sqlTable.Columns["NC12_wyrobu"].ColumnName = "Model";

            Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>> result = new Dictionary<DateTime, SortedDictionary<int, Dictionary<string, DataTable>>>();
            foreach (DataRow row in sqlTable.Rows)
            {
                DateTime inspTime = DateTime.Parse(row["Data"].ToString());
                dateShiftNo shiftInfo = SMTOperations.whatDayShiftIsit(inspTime);
                string model = row["Model"].ToString();

                if (!result.ContainsKey(shiftInfo.date.Date))
                {
                    result.Add(shiftInfo.date.Date, new SortedDictionary<int, Dictionary<string, DataTable>>());
                }
                if (!result[shiftInfo.date.Date].ContainsKey(shiftInfo.shift))
                {
                    result[shiftInfo.date.Date].Add(shiftInfo.shift, new Dictionary<string, DataTable>());
                }
                if (!result[shiftInfo.date.Date][shiftInfo.shift].ContainsKey(model))
                {
                    result[shiftInfo.date.Date][shiftInfo.shift].Add(model, sqlTable.Clone());

                }
                result[shiftInfo.date.Date][shiftInfo.shift][model].Rows.Add(row.ItemArray);
            }
            return result;
        }

        public static Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> GetTestMeasurements (int daysAgo)
        {
            DataTable sqlTable = new DataTable();
            string untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6).ToString("yyyy-MM-dd") ;

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT serial_no,inspection_time,wip_entity_name,tester_id,result FROM tb_tester_measurements WHERE inspection_time>@until and tester_id<>'0' order by inspection_time;");
            command.Parameters.AddWithValue("@until", untilDay);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            sqlTable.Columns["inspection_time"].ColumnName = "Data";
            sqlTable.Columns["tester_id"].ColumnName = "Tester";
            //sqlTable.Columns["NC12_wyrobu"].ColumnName = "Model";
            sqlTable.Columns["serial_no"].ColumnName = "PCB";
            sqlTable.Columns["wip_entity_name"].ColumnName = "LOT";

            Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>> result = new Dictionary<DateTime, SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>>();
            foreach (DataRow row in sqlTable.Rows)
            {
                string lineID = row["Tester"].ToString();
                string testerID = "";
                switch (lineID)
                {
                    case "1":
                        {
                            testerID = "Optical";
                            break;
                        }
                    case "2":
                        {
                            testerID = "Manual-2";
                            break;
                        }
                    case "3":
                        {
                            testerID = "Manual-1";
                            break;
                        }
                    case "4":
                        {
                            testerID = "test_SMT5";
                            break;
                        }
                    case "5":
                        {
                            testerID = "test_SMT6";
                            break;
                        }
                }
                if (testerID == "") continue;

                DateTime inspTime = DateTime.Parse(row["Data"].ToString());
                dateShiftNo shiftInfo = SMTOperations.whatDayShiftIsit(inspTime);
                string lot = row["LOT"].ToString();
                
                if (!result.ContainsKey(shiftInfo.date.Date))
                {
                    result.Add(shiftInfo.date.Date, new SortedDictionary<int, Dictionary<string, Dictionary<string, DataTable>>>());
                }
                if (!result[shiftInfo.date.Date].ContainsKey(shiftInfo.shift))
                {
                    result[shiftInfo.date.Date].Add(shiftInfo.shift, new Dictionary<string, Dictionary<string, DataTable>>());
                }
                if (!result[shiftInfo.date.Date][shiftInfo.shift].ContainsKey(testerID))
                {
                    result[shiftInfo.date.Date][shiftInfo.shift].Add(testerID, new Dictionary<string, DataTable>());
                }
                if (!result[shiftInfo.date.Date][shiftInfo.shift][testerID].ContainsKey(lot))
                {
                    result[shiftInfo.date.Date][shiftInfo.shift][testerID].Add(lot, sqlTable.Clone());
                }

                result[shiftInfo.date.Date][shiftInfo.shift][testerID][lot].Rows.Add(row.ItemArray);
            }

            return result;
        }

        public static DataTable GetSmtRecordsFromDbQuantityOnly(int daysAgo)
        {
            DataTable result = new DataTable();
            DateTime untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6);

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc,KoncowkiLED FROM MES.dbo.tb_SMT_Karta_Pracy WHERE DataCzasKoniec>@until order by [DataCzasKoniec];");
            //command.Parameters.AddWithValue("@qty", recordsQty);
            //command.Parameters.AddWithValue("@smtLine", line);
            command.Parameters.AddWithValue("@until", untilDay);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);

            return result;
        }

        public static Dictionary<string,string> lotToSmtLine(int daysAgo)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            DataTable sqlTable = new DataTable();
            DateTime untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6);

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasKoniec,LiniaSMT,NrZlecenia FROM MES.dbo.tb_SMT_Karta_Pracy WHERE DataCzasKoniec>@until order by [DataCzasKoniec];");
            command.Parameters.AddWithValue("@until", untilDay);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                if (!result.ContainsKey(row["NrZlecenia"].ToString()))
                {
                    result.Add(row["NrZlecenia"].ToString(), row["LiniaSMT"].ToString());
                }
            }

            return result;
        }
    }
}
