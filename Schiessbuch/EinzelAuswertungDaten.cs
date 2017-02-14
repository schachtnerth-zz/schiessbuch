using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace schiessbuch
{
    class EinzelAuswertungDaten
    {
        public class Schussverteilung
        {
            public int iAnzZehner;
            public int iAnzNeuner;
            public int iAnzAchter;
            public int iAnzSiebener;
            public int iAnzSechser;
            public int iAnzFuenfer;
            public int iAnzVierer;
            public int iAnzDreier;
            public int iAnzZweier;
            public int iAnzEinser;
            public int iAnzNuller;
            public int iInnenzehner;
        };

        public class SchussWert
        {
            public int iSchussNummer;
            public int iSchussWert;
        };

        public class SerienAuswertung
        {
            public class TrefferInSerie
            {
                public int iSchussNummer;
                public float fWertung;
                public float fWinkel;
                public int iRing;
                public bool bInnenZehner;
                public float xrahmeninmm;
                public float yrahmeninmm;
            }
            public int iSerienNr;
            public List<TrefferInSerie> treffer;
            public SchussWert[] beste;
            public float fTrefferlage_x;
            public float fTrefferlage_y;
            public float fTrefferlage_r;
            public float fVarianz_x;
            public float fVarianz_y;
            public float fVarianz_r;
            public float fStabw_x;
            public float fStabw_y;
            public float fStabw_r;
            public int iSerienSumme;
        };

        public int iStandNummer;//
        public string strName;//
        public string strVorname;//
        public string strVerein;//
        public string strDatum;//
        public string strUhrzeit;//
        public string strDisziplin;//
        public Schussverteilung schussverteilung;//
        public SchussWert[] schlechteste;//
        public SchussWert[] beste;//
        public float fTrefferlage_x;
        public float fTrefferlage_y;
        public float fTrefferlage_r;
        public float fVarianz_x;
        public float fVarianz_y;
        public float fVarianz_r;
        public float fStabw_x;
        public float fStabw_y;
        public float fStabw_r;

        public int iErgebnisRing; //
        public float fErgebnisZehntel;//
        public List<SerienAuswertung> serien;
        public List<SerienAuswertung.TrefferInSerie> alleSchuss;
    }
}
