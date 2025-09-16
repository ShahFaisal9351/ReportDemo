using Npgsql;
using System.Data;

public class DatabaseHelper
{
    public static DataTable GetStudentData()
    {
        // Replace with your PostgreSQL details
        string connString = "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=ReportDemoDB";


        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            string query = "SELECT id, name, age FROM student"; // Your table name
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
