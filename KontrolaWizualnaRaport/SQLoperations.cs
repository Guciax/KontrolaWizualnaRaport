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

        public static DataTable DownloadFromSQL()
        {
            HashSet<string> result = new HashSet<string>();
            DataTable tabletoFill = new DataTable();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText = @"SELECT Id,Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy;";
            //@"SELECT Data_czas,Operator,iloscDobrych,numerZlecenia,ngBrakLutowia,ngBrakDiodyLed,ngBrakResConn,ngPrzesuniecieLed,ngPrzesuniecieResConn,ngZabrudzenieLed,ngUszkodzenieMechaniczneLed,ngUszkodzenieConn,ngWadaFabrycznaDiody,ngUszkodzonePcb,ngWadaNaklejki,ngSpalonyConn,ngInne,scrapBrakLutowia,scrapBrakDiodyLed,scrapBrakResConn,scrapPrzesuniecieLed,scrapPrzesuniecieResConn,scrapZabrudzenieLed,scrapUszkodzenieMechaniczneLed,scrapUszkodzenieConn,scrapWadaFabrycznaDiody,scrapUszkodzonePcb,scrapWadaNaklejki,scrapSpalonyConn,scrapInne,ngTestElektryczny FROM tb_Kontrola_Wizualna_Karta_Pracy WHERE Data_czas > '" + DateTime.Now.AddDays(-90).ToShortDateString() + "';";

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

        public static Dictionary<string,string>[] LotList()
        {
            Dictionary<string, string> result1 = new Dictionary<string, string>();
            Dictionary<string, string> result2 = new Dictionary<string, string>();

            DataTable sqlTable = new DataTable();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = @"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;";

            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            command.CommandText =
                @"SELECT Nr_Zlecenia_Produkcyjnego,NC12_wyrobu,Ilosc_wyrobu_zlecona FROM tb_Zlecenia_produkcyjne;";

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(sqlTable);

            foreach (DataRow row in sqlTable.Rows)
            {
                if (result1.ContainsKey(row["Nr_Zlecenia_Produkcyjnego"].ToString())) continue;

                result1.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["NC12_wyrobu"].ToString().Replace("LLFML",""));
                result2.Add(row["Nr_Zlecenia_Produkcyjnego"].ToString(), row["Ilosc_wyrobu_zlecona"].ToString());
            }

            Dictionary<string, string>[] result = new Dictionary<string, string>[] { result1, result2 };


            return result;
        }
    }
}
