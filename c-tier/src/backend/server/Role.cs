using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_tier.src.backend.server
{
    public class Role
    {
        public ulong roleID;
        public string roleName;
        public int permLevel = 1; // 1 - 9
        public string roleColor;
        public bool isDefault;
      public Role(string roleName, int permLevel, string roleColor)
        {
            this.roleID = Utils.GenerateID(6);
            this.roleName = roleName;
            this.permLevel = Math.Clamp(permLevel, 1, 9);
            this.roleColor = roleColor;
        }
        public Role(string roleName, int permLevel, string roleColor, bool isDefault)
        {
            this.roleID = Utils.GenerateID(6);
            this.roleName = roleName;
            this.permLevel = Math.Clamp(permLevel, 1, 9);
            this.roleColor = roleColor;
            this.isDefault = isDefault;
        }

    }
}
