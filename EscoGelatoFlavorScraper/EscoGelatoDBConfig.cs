using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscoGelatoFlavorScraper
{
    internal static class EscoGelatoDBConfig
    {
        private static readonly string? dbServerEndpoint;
        private static readonly string? dbServerPortNum;
        private static readonly string? dbName;
        private static readonly string? userName;
        private static readonly string? userPassword;

        public static string? DbServerEndpoint => dbServerEndpoint;
        public static string? DbServerPortNum => dbServerPortNum;
        public static string? DbName => dbName;
        public static string? UserName => userName;
        public static string? UserPassword => userPassword;

        static EscoGelatoDBConfig()
        {
            dbServerEndpoint = Environment.GetEnvironmentVariable("EscoGelato_DB_Endpoint");
            dbServerPortNum = Environment.GetEnvironmentVariable("EscoGelato_DB_Port");
            dbName = Environment.GetEnvironmentVariable("EscoGelato_DB_Name");
            userName = Environment.GetEnvironmentVariable("EscoGelato_DB_UserName");
            userPassword = Environment.GetEnvironmentVariable("EscoGelato_DB_Password");
        }

    }
}
