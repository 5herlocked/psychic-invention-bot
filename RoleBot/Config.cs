//@author Shardul Vaidya

using System.Collections.Generic;
using System.Xml.Serialization;

namespace RoleBot
{
    [XmlInclude(typeof(List<RoleWatch>))]
    public class Config
    {
        public string Token { get; set; }
        
        public bool AutoRemoveFlag { get; set; }

        [XmlElement("Prefix")]
        public string CommandPrefix { get; set; }

        [XmlElement("Role")]
        public List<RoleWatch> RolesToWatch { get; set; }

        public Config () { RolesToWatch = new List<RoleWatch>(); }
    }
}