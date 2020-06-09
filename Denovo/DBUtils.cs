using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denovo
{
    class DBUtils
    {
        public static SqlConnection GetDBConnection()
        {
            return DBConnection.GetDBConnection("192.168.8.121\\MSSQLSERVER,1433", "DeNovo", "User01", "12345");
        }
    }
}
