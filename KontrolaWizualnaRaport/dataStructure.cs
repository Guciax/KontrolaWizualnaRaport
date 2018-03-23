using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    class dataStructure
    {
        public dataStructure(int id, DateTime fixedDateTime, DateTime realDateTime, int shiftNumber, string oper, int goodQty,int allQty,int allNg, int allScrap,string numerZlecenia,int ngBrakLutowia,int ngBrakDiodyLed,int ngBrakResConn,int ngPrzesuniecieLed,int ngPrzesuniecieResConn,int ngZabrudzenieLed,int ngUszkodzenieMechaniczneLed,int ngUszkodzenieConn,int ngWadaFabrycznaDiody,int ngUszkodzonePcb,int ngWadaNaklejki,int ngSpalonyConn,int ngInne,int scrapBrakLutowia,int scrapBrakDiodyLed,int scrapBrakResConn,int scrapPrzesuniecieLed,int scrapPrzesuniecieResConn,int scrapZabrudzenieLed,int scrapUszkodzenieMechaniczneLed,int scrapUszkodzenieConn,int scrapWadaFabrycznaDiody,int scrapUszkodzonePcb,int scrapWadaNaklejki,int scrapSpalonyConn,int scrapInne,int ngTestElektryczny, string smtLine)
        {
            sqlId = id;
            FixedDateTime = fixedDateTime;
            RealDateTime = realDateTime;
            ShiftNumber = shiftNumber;
            Oper = oper;
            GoodQty = goodQty;
            AllQty = allQty;
            AllNg = allNg;
            AllScrap = allScrap;
            NumerZlecenia = numerZlecenia;

            NgBrakLutowia = ngBrakLutowia;
            NgBrakDiodyLed = ngBrakDiodyLed;
            NgBrakResConn = ngBrakResConn;
            NgPrzesuniecieLed = ngPrzesuniecieLed;
            NgPrzesuniecieResConn = ngPrzesuniecieResConn;
            NgZabrudzenieLed = ngZabrudzenieLed;
            NgUszkodzenieMechaniczneLed = ngUszkodzenieMechaniczneLed;
            NgUszkodzenieConn = ngUszkodzenieConn;
            NgWadaFabrycznaDiody = ngWadaFabrycznaDiody;
            NgUszkodzonePcb = ngUszkodzonePcb;
            NgWadaNaklejki = ngWadaNaklejki;
            NgSpalonyConn = ngSpalonyConn;
            NgInne = ngInne;
            ScrapBrakLutowia = scrapBrakLutowia;
            ScrapBrakDiodyLed = scrapBrakDiodyLed;
            ScrapBrakResConn = scrapBrakResConn;
            ScrapPrzesuniecieLed = scrapPrzesuniecieLed;
            ScrapPrzesuniecieResConn = scrapPrzesuniecieResConn;
            ScrapZabrudzenieLed = scrapZabrudzenieLed;
            ScrapUszkodzenieMechaniczneLed = scrapUszkodzenieMechaniczneLed;
            ScrapUszkodzenieConn = scrapUszkodzenieConn;
            ScrapWadaFabrycznaDiody = scrapWadaFabrycznaDiody;
            ScrapUszkodzonePcb = scrapUszkodzonePcb;
            ScrapWadaNaklejki = scrapWadaNaklejki;
            ScrapSpalonyConn = scrapSpalonyConn;
            ScrapInne = scrapInne;
            NgTestElektryczny = ngTestElektryczny;
            SmtLine = smtLine;
        }

        public int sqlId { get; }
        public DateTime FixedDateTime { get; }
        public DateTime RealDateTime { get; }
        public int ShiftNumber { get; }
        public string Oper { get; }
        public int GoodQty { get; }
        public int AllQty { get; }
        public int AllNg { get; }
        public int AllScrap { get; }
        public string NumerZlecenia { get; }
        

        public int NgBrakLutowia { get; }
        public int NgBrakDiodyLed { get; }
        public int NgBrakResConn { get; }
        public int NgPrzesuniecieLed { get; }
        public int NgPrzesuniecieResConn { get; }
        public int NgZabrudzenieLed { get; }
        public int NgUszkodzenieMechaniczneLed { get; }
        public int NgUszkodzenieConn { get; }
        public int NgWadaFabrycznaDiody { get; }
        public int NgUszkodzonePcb { get; }
        public int NgWadaNaklejki { get; }
        public int NgSpalonyConn { get; }
        public int NgInne { get; }

        public int ScrapBrakLutowia { get; }
        public int ScrapBrakDiodyLed { get; }
        public int ScrapBrakResConn { get; }
        public int ScrapPrzesuniecieLed { get; }
        public int ScrapPrzesuniecieResConn { get; }
        public int ScrapZabrudzenieLed { get; }
        public int ScrapUszkodzenieMechaniczneLed { get; }
        public int ScrapUszkodzenieConn { get; }
        public int ScrapWadaFabrycznaDiody { get; }
        public int ScrapUszkodzonePcb { get; }
        public int ScrapWadaNaklejki { get; }
        public int ScrapSpalonyConn { get; }
        public int ScrapInne { get; }

        public int NgTestElektryczny { get; }
        public string SmtLine { get; set; }
    }
}
