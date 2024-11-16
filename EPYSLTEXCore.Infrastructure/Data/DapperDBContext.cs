using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Data
{
    public class DapperDBContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connnectionString;
        public DapperDBContext(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connnectionString = this._configuration.GetConnectionString("DBConnection");
            //this._connnectionString = AppConstants.ConnectionString;
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connnectionString);
    }
}
