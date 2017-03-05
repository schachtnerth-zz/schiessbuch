using System.Drawing;

namespace schiessbuch
{
    class Ergebnisbild
    {
        public Bitmap bild;
        public float fMaxX, fMaxY;
        bool bIsValid;
        public bool IsValid() { return bIsValid; }
        public void SetValid(bool bValid) {
            bIsValid = bValid;
            if (bValid) bIsChanged = true;
        }
        public bool bIsChanged { get; set; }
        public float maxAbstand;
        public Ergebnisbild()
        {
            bIsValid = false;
        }
    }
}
