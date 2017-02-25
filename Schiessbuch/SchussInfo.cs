namespace schiessbuch
{
    using System;

    internal class SchussInfo
    {
        public int ring;
        public int schussnummer;
        public float xrahmeninmm;
        public float yrahmeninmm;
        public string strZielscheibe;
        public int schuetze;
        public string disziplin;
        public bool probe;

        public SchussInfo(float xrahmeninmm, float yrahmeninmm, int ring, int schussnummer, string strZielscheibe, int schuetze, string disziplin, bool probe)
        {
            this.xrahmeninmm = xrahmeninmm;
            this.yrahmeninmm = yrahmeninmm;
            this.schussnummer = schussnummer;
            this.ring = ring;
            this.strZielscheibe = strZielscheibe;
            this.schuetze = schuetze;
            this.disziplin = disziplin;
            this.probe = probe;
        }
    }
}

