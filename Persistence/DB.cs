using Microsoft.Data.SqlClient;
using System.Data;

namespace CarteiraDB.Persistence
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;
    
    public class DB
    {
        
        

        
                private readonly string _connectionString;

                public DB(IConfiguration configuration)
                {
                    _connectionString = configuration.GetConnectionString("DefaultConnection");
                }


                public IDbConnection GetConnection()
                {
                    var connection = new SqlConnection(_connectionString);
                    connection.Open();
                    return connection;
                }



    }
}
