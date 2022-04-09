using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Velusia.Server.ActiveDirectory
{
    public static class AdExtensions
    {
        public static string[] GetGroupList(this UserPrincipal principal)
        {
            var groups = new List<string>();
            foreach (var group in principal.GetGroups())
            {
                var groupName = group.Name;
                group.Dispose();

                groups.Add(groupName);
            }

            return groups.ToArray();
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
