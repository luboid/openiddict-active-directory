using System.DirectoryServices.AccountManagement;

namespace Velusia.Server.ActiveDirectory
{
    public record AdSettings
    {
        public ContextType ContextType { get; set; }

        public string Server { get; set; }

        public string[] AllowGroups { get; set; }

        public AdRole[] Roles { get; set; }
    }
}
