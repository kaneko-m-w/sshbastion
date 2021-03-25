using System;
using Renci.SshNet;
using System.Data.SqlClient;
using System.Data;

namespace sshbastion
{
    class Program
    {
        static void Main(string[] args)
        {
            // 踏み台サーバのホスト名
            string bastionServer = "XXXXXXXXXX";
            // 踏み台サーバのアカウント名
            string userName = "XXXXXXXXXX";
            // 踏み台サーバのパスワード
            string password = "XXXXXXXXXX";

            ConnectionInfo info = new ConnectionInfo(bastionServer, 22, userName,
                new AuthenticationMethod[] {
                        new PasswordAuthenticationMethod(userName, password)
                }
            );

            // SQL Server 接続文字列
            string connectionString = @"Data Source=127.0.0.1;Integrated Security=False;User ID=XXXXXXXXXX;Password=XXXXXXXXXX";
            // データベースサーバのホスト名
            string dbServer = "XXXXXXXXXX";

            using (var client = new SshClient(info))
            {
                client.Connect();
                var forward = new ForwardedPortLocal("127.0.0.1", 1433, dbServer, 1433);
                client.AddForwardedPort(forward);
                forward.Start();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        try
                        {
                            connection.Open();

                            command.CommandText = @"SELECT count(*) AS count FROM employee";

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.Message);
                            throw;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    forward.Stop();
                }
            }
        }
    }
}
