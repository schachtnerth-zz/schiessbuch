using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace schiessbuch
{
    class disziplin
    {
        public disziplin(string disziplinId, string disziplinName, XElement xmlElement)
        {
            this.disziplinId = disziplinId;
            this.disziplinName = disziplinName;
            this.xmlElement = xmlElement;
        }
        public bool Equals(disziplin obj)
        {
            if (obj.disziplinId == this.disziplinId && obj.disziplinName == this.disziplinName)
                return true;
            else
                return false;
        }
        public override string ToString()
        {
            return this.disziplinName;
        }
        public string disziplinName;
        public string disziplinId;
        public XElement xmlElement;
    }
}
