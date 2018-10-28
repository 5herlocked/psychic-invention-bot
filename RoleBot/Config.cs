//@author Shardul Vaidya

using System.Collections.Generic;
using System.Xml.Serialization;

namespace RoleBot
{
    public class Config
    {
        public string Token { get; set; }
        
        public bool AutoRemoveFlag { get; set; }

        [XmlElement("Prefix")]
        public string CommandPrefix { get; set; }

        public List<RoleWatch> RolesToWatch { get; set; }

        public Config () { RolesToWatch = new List<RoleWatch>(); }
    }
}