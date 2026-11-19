using System.Data.SqlClient;

namespace ChudoObuv
{
    public static class Db
    {
        private const string ConnectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=ChudoObuv;Integrated Security=True";

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static string ReadString(SqlDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                return string.Empty;
            }

            return reader.GetString(index);
        }
    }
}