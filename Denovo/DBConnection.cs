using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denovo
{
    class DBConnection
    {
        public static SqlConnection GetDBConnection(string datasource, string database, string username, string password)
        {
            return new SqlConnection("Data Source=" + datasource + ";Initial Catalog=" + database + ";Persist Security Info=False;User ID=" + username + ";Password=" + password);
        }
    }
}
