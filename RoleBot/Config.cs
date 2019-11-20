//@author Shardul Vaidya

using System.Collections.Generic;

namespace RoleBot
{
    public class Config
    {
        public string Token { get; set; }
        
        public bool AutoRemoveFlag { get; set; }

        public string CommandPrefix { get; set; }

        // TODO implement HashMap to make this cleaner
        public HashSet<RoleWatch> RolesToWatch { get; set; }

        public Config() { RolesToWatch = new HashSet<RoleWatch>(); }
    }
}