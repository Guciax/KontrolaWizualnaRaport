using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public static DataTable GetSmtRecordsFromDbQuantityOnly(int daysAgo)
        {
            DataTable result = new DataTable();
            DateTime untilDay = DateTime.Now.Date.AddDays(daysAgo * (-1)).AddHours(6);

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = String.Format(@"SELECT DataCzasStart,DataCzasKoniec,LiniaSMT,OperatorSMT,NrZlecenia,Model,IloscWykonana,NGIlosc,ScrapIlosc FROM MES.dbo.tb_SMT_Karta_Pracy WHERE DataCzasKoniec>@until order by [DataCzasKoniec];");
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
