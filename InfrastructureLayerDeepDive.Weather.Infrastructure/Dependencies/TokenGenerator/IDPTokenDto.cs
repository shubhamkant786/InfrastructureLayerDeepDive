using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.TokenGenerator
{
    public class IDPTokenDto
    {
        public string? Access_token { get; set; }
        public string? Refresh_token { get; set; }
        public string? Id_token { get; set; }
        public string? Scope { get; set; }
        public string? Token_type { get; set; }
        public int? Expires_in { get; set; }
    }
}
