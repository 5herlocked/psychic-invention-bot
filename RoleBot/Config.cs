//@author Shardul Vaidya

using System;
using System.Collections.Generic;

namespace RoleBot
{
    public class Config
    {
        public string Token { get; set; }
        
        public bool AutoRemoveFlag { get; set; }

        public string CommandPrefix { get; set; }

        // TODO implement HashMap to make this cleaner
        public List<RoleWatch> RolesToWatch { get; set; }

        public Config() { RolesToWatch = new List<RoleWatch>(); }
    }
}