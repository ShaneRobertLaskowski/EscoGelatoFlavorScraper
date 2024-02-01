using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace EscoGelatoFlavorScraper
{
    //Credit: https://stackoverflow.com/questions/21618015/how-to-connect-to-mysql-database#comment91780671_21618283
    internal class MyDbConnectionClass
    {
        internal MyDbConnectionClass()
        {
            //does nothing
        }

        public MyDbConnectionClass(string? server, string? databaseName, string? userName, 
            string? password, string? port)
        {
            Server = server;
            DatabaseName = databaseName;
            UserName = userName;
            Password = password;
            Port = port;
        }

        public string? Server { get; set; }
        public string? DatabaseName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Port { get; set; }

        internal MySqlConnection? Connection { get; set;}

        private static MyDbConnectionClass? _instance = null;

        //this singleton code doesnt seem to be done correctly
        public static MyDbConnectionClass Instance()
        {
            // ??= is same as checking if _instance is null then assigning it the object if it is null.
            //this helps ensure that no more than one Object is created when Instance() method is called.
            _instance ??= new MyDbConnectionClass(); 
            return _instance;
        }
    
        /// <summary>
        /// Perhaps this method should be split up
        /// </summary>
        /// <returns></returns>
        /// <issue>need to do validation check on all inputs for DB connection,
        ///     not just DatabaseName</issue>
        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;
                Connection = new MySqlConnection($"Server={Server};Port={Port};User ID=" +
                    $"{UserName};Password={Password};Database={DatabaseName}");
                Connection.Open();
            }
            else if (Connection.State == System.Data.ConnectionState.Closed) { Connection.Open(); }
            //this Else-if above is incase we wish to re-use the connection... just do IsConnect() method in you program again
            //https://stackoverflow.com/questions/21618015/how-to-connect-to-mysql-database#comment91780671_21618283
            //Credit: Larphoid

            return true;
        }

        public void Close()
        {
            if (Connection != null)
                Connection.Close();
        }
    }
}
