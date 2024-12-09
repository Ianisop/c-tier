

namespace c_tier.src.backend.server
{
    public class Role
    {
        public ulong roleID;
        public string roleName;
        public int permLevel = 1; //clamped between 1 - 9, 9 being highest
        public string roleColor;
        public bool isDefault;

        public Role()
        {

        }

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
