using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Repositories.SqlServer
{
    public class SqlServerDbOptions
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Database { get; set; }

        public string? Host { get; set; }

        public string? Port { get; set; }

        public int? Timeout { get; set; }
    }
}
