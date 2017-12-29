using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    class dataLoader
    {
        public static List<dataStructure> LoadData(DataTable inputTable)
        {
            List<dataStructure> result = new List<dataStructure>();

            foreach (DataRow row in inputTable.Rows)
            {
                int id;
                DateTime fixedDateTime;
                DateTime realDateTime;
                int shiftNumber;
                string oper;
                int goodQty;
                int allQty=0;
                string numerZlecenia;
                int allNg = 0;
                int allScrap = 0;

                int NgBrakLutowia;
                int NgBrakDiodyLed;
                int NgBrakResConn;
                int NgPrzesuniecieLed;
                int NgPrzesuniecieResConn;
                int NgZabrudzenieLed;
                int NgUszkodzenieMechaniczneLed;
                int NgUszkodzenieConn;
                int NgWadaFabrycznaDiody;
                int NgUszkodzonePcb;
                int NgWadaNaklejki;
                int NgSpalonyConn;
                int NgInne;

                int ScrapBrakLutowia;
                int ScrapBrakDiodyLed;
                int ScrapBrakResConn;
                int ScrapPrzesuniecieLed;
                int ScrapPrzesuniecieResConn;
                int ScrapZabrudzenieLed;
                int ScrapUszkodzenieMechaniczneLed;
                int ScrapUszkodzenieConn;
                int ScrapWadaFabrycznaDiody;
                int ScrapUszkodzonePcb;
                int ScrapWadaNaklejki;
                int ScrapSpalonyConn;
                int ScrapInne;

                int NgTestElektryczny;

                id = int.Parse(row["Id"].ToString());
                realDateTime = ParseExact(row["Data_czas"].ToString());
                fixedDateTime = FixedShiftDate(realDateTime);
                shiftNumber = DateToShiftNumber(realDateTime);
                oper = row["Operator"].ToString();
                goodQty = int.Parse(row["iloscDobrych"].ToString());
                allQty = goodQty;
                numerZlecenia = row["numerZlecenia"].ToString();

                 NgBrakLutowia = int.Parse(row["NgBrakLutowia"].ToString());
                allQty += NgBrakLutowia;
                allNg += NgBrakLutowia;
                NgBrakDiodyLed = int.Parse(row["NgBrakDiodyLed"].ToString());
                allQty += NgBrakDiodyLed;
                allNg += NgBrakDiodyLed;
                NgBrakResConn = int.Parse(row["NgBrakResConn"].ToString());
                allQty += NgBrakResConn;
                allNg += NgBrakResConn;
                NgPrzesuniecieLed = int.Parse(row["NgPrzesuniecieLed"].ToString());
                allQty += NgPrzesuniecieLed;
                allNg += NgPrzesuniecieLed;
                NgPrzesuniecieResConn = int.Parse(row["NgPrzesuniecieResConn"].ToString());
                allQty += NgPrzesuniecieResConn;
                allNg += NgPrzesuniecieResConn;
                NgZabrudzenieLed = int.Parse(row["NgZabrudzenieLed"].ToString());
                allQty += NgZabrudzenieLed;
                allNg += NgZabrudzenieLed;
                NgUszkodzenieMechaniczneLed = int.Parse(row["NgUszkodzenieMechaniczneLed"].ToString());
                allQty += NgUszkodzenieMechaniczneLed;
                allNg += NgUszkodzenieMechaniczneLed;
                NgUszkodzenieConn = int.Parse(row["NgUszkodzenieConn"].ToString());
                allQty += NgUszkodzenieConn;
                allNg += NgUszkodzenieConn;
                NgWadaFabrycznaDiody = int.Parse(row["NgWadaFabrycznaDiody"].ToString());
                allQty += NgWadaFabrycznaDiody;
                allNg += NgWadaFabrycznaDiody;
                NgUszkodzonePcb = int.Parse(row["NgUszkodzonePcb"].ToString());
                allQty += NgUszkodzonePcb;
                allNg += NgUszkodzonePcb;
                NgWadaNaklejki = int.Parse(row["NgWadaNaklejki"].ToString());
                allQty += NgWadaNaklejki;
                allNg += NgWadaNaklejki;
                NgSpalonyConn = int.Parse(row["NgSpalonyConn"].ToString());
                allQty += NgSpalonyConn;
                allNg += NgSpalonyConn;
                NgInne = int.Parse(row["NgInne"].ToString());
                allQty += NgInne;
                allNg += NgInne;

                ScrapBrakLutowia = int.Parse(row["ScrapBrakLutowia"].ToString());
                allQty += ScrapBrakLutowia;
                allScrap += ScrapBrakLutowia;
                ScrapBrakDiodyLed = int.Parse(row["ScrapBrakDiodyLed"].ToString());
                allQty += ScrapBrakDiodyLed;
                allScrap += ScrapBrakDiodyLed;
                ScrapBrakResConn = int.Parse(row["ScrapBrakResConn"].ToString());
                allQty += ScrapBrakResConn;
                allScrap += ScrapBrakResConn;
                ScrapPrzesuniecieLed = int.Parse(row["ScrapPrzesuniecieLed"].ToString());
                allQty += ScrapPrzesuniecieLed;
                allScrap += ScrapPrzesuniecieLed;
                ScrapPrzesuniecieResConn = int.Parse(row["ScrapPrzesuniecieResConn"].ToString());
                allQty += ScrapPrzesuniecieResConn;
                allScrap += ScrapPrzesuniecieResConn;
                ScrapZabrudzenieLed = int.Parse(row["ScrapZabrudzenieLed"].ToString());
                allQty += ScrapZabrudzenieLed;
                allScrap += ScrapZabrudzenieLed;
                ScrapUszkodzenieMechaniczneLed = int.Parse(row["ScrapUszkodzenieMechaniczneLed"].ToString());
                allQty += ScrapUszkodzenieMechaniczneLed;
                allScrap += ScrapUszkodzenieMechaniczneLed;
                ScrapUszkodzenieConn = int.Parse(row["ScrapUszkodzenieConn"].ToString());
                allQty += ScrapUszkodzenieConn;
                allScrap += ScrapUszkodzenieConn;
                ScrapWadaFabrycznaDiody = int.Parse(row["ScrapWadaFabrycznaDiody"].ToString());
                allQty += ScrapWadaFabrycznaDiody;
                allScrap += ScrapWadaFabrycznaDiody;
                ScrapUszkodzonePcb = int.Parse(row["ScrapUszkodzonePcb"].ToString());
                allQty += ScrapUszkodzonePcb;
                allScrap += ScrapUszkodzonePcb;
                ScrapWadaNaklejki = int.Parse(row["ScrapWadaNaklejki"].ToString());
                allQty += ScrapWadaNaklejki;
                allScrap += ScrapWadaNaklejki;
                ScrapSpalonyConn = int.Parse(row["ScrapSpalonyConn"].ToString());
                allQty += ScrapSpalonyConn;
                allScrap += ScrapSpalonyConn;
                ScrapInne = int.Parse(row["ScrapInne"].ToString());
                allQty += ScrapInne;
                allScrap += ScrapInne;

                NgTestElektryczny = int.Parse(row["NgTestElektryczny"].ToString());
                allQty += NgTestElektryczny;
                allNg += NgTestElektryczny;

                //Debug.WriteLine(allQty + " " + allNg + " " + allScrap);

                dataStructure recordToAdd = new dataStructure(id, fixedDateTime, realDateTime, shiftNumber, oper, goodQty, allQty,allNg,allScrap, numerZlecenia, NgBrakLutowia, NgBrakDiodyLed, NgBrakResConn, NgPrzesuniecieLed, NgPrzesuniecieResConn, NgZabrudzenieLed, NgUszkodzenieMechaniczneLed, NgUszkodzenieConn, NgWadaFabrycznaDiody, NgUszkodzonePcb, NgWadaNaklejki, NgSpalonyConn, NgInne, ScrapBrakLutowia, ScrapBrakDiodyLed, ScrapBrakResConn, ScrapPrzesuniecieLed, ScrapPrzesuniecieResConn, ScrapZabrudzenieLed, ScrapUszkodzenieMechaniczneLed, ScrapUszkodzenieConn, ScrapWadaFabrycznaDiody, ScrapUszkodzonePcb, ScrapWadaNaklejki, ScrapSpalonyConn, ScrapInne, NgTestElektryczny);
                result.Add(recordToAdd);
            }

            return result;
        }

        public static DateTime ParseExact(string date)
        {
            try
            {
                if (date.Contains("-"))
                    return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                if (date.Contains(@"/"))
                    return DateTime.ParseExact(date, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                else
                    return DateTime.ParseExact(date, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
            }
            catch (Exception e)
            {
                return new DateTime(1900, 1, 1);
            }
        }
        public static DateTime FixedShiftDate(DateTime inputDate)
        {
            if (inputDate.Hour >= 22)
            {
                return inputDate.AddDays(1);

            }
            else return inputDate;
        }

        public static int DateToShiftNumber(DateTime inputDate)
        {
            if (inputDate.Hour >= 22)
            {
                return  3;
            }
            if (inputDate.Hour >= 14) return 2;
            if (inputDate.Hour >= 6) return 1;

            return 0;
        }
    }

}
