using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Velusia.Server.Contracts
{
    public record AuthenticationSettings
    {
        public string Scheme { get; init; }
    }
}
