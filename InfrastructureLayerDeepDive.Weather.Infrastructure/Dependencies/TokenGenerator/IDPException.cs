using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Dependencies.TokenGenerator
{
    public class IDPException : Exception
    {
        public IDPException()
        {
        }

        public IDPException(string? message) : base(message)
        {
        }

        public IDPException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
