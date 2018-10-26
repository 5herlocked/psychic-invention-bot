//@author Shardul Vaidya

using System.Collections.Generic;
using System.Xml.Serialization;

namespace RoleBot
{
    [XmlInclude(typeof(RoleWatch))]
    public class Config
    {
        [XmlElement("token")]
        public string Token { get; set; }    

        [XmlElement("autoremove")]
        public bool AutoRemoveFlag { get; set; } 
        
        [XmlElement("role")]
        public List<RoleWatch> RolesToWatch = new List<RoleWatch>();
    }
}