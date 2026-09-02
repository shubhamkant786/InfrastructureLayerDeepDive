using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.TokenGenerator
{
    public interface ITokenGenerator
    {
        Task<string> AuthenticateAndGenerateToken();
    }
}
