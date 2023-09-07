using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Telegram.Bot;

namespace SqlServer.DataBase
{
    public class Conexao : IDisposable
    {
        //put your database configuration in the app config, after that input the config section in below(PlaceHolder)
#if DEBUG //Usually we have differents connection string for debug and release
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["PlaceHolder"].ConnectionString);
#else
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["PlaceHolder"].ConnectionString);
#endif

        //put a telegram bot token, so a message will be send wherever the connection fails
        private static readonly TelegramBotClient Bot = new TelegramBotClient("placeholder");

        public SqlConnection Conectar()//Open connection
        {
            int tries = 0;
            Cursor.Current = Cursors.WaitCursor;
            while (con.State == ConnectionState.Closed)
                try
                {
                    if (con.State == ConnectionState.Closed)//Check if the connection is already open                      
                    {
                        con.Open();
                        Cursor.Current = Cursors.Default;
                        return con;
                    }
                    return con;
                }
                catch (SqlException ex)
                {
                    if (tries == 13)
                    {
                        Bot.SendTextMessageAsync(207898158, "Connection Failed");
                        
                        throw new System.Runtime.Remoting.ServerException("Connection with Data Base Failed", ex);
                    }
                }
            return con;
        }
        public SqlConnection Desconectar()//Fecha a conexão
        {
            if (con.State == ConnectionState.Open)
                con.Close();

            return con;
        }

        public void Dispose()
        {
            if (con != null)
            {
                Desconectar();
            }
        }
    }
}
