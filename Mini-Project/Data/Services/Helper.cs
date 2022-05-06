using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Data.Services
{
    public class Helper
    {
        private OdbcConnection _odbcConnection;
        public Helper(IConfiguration configuration)
        {
            var Configuration = configuration;
            var ConnectionString = Configuration.GetConnectionString("VerticaConnectionString");
            _odbcConnection = new OdbcConnection(ConnectionString);
        }
        public string getDateFromFileName(string file)
        {
            string[] parsedFileName = Path.GetFileNameWithoutExtension(file).Split("_");
            string dateTimeKey = parsedFileName[parsedFileName.Length - 2] + parsedFileName.Last();
            DateTime dateTimeColumn = DateTime.ParseExact(dateTimeKey, "yyyyMMddHHmmss", null);
            return dateTimeColumn.ToString();
        }
        public bool checkForPresentFiles(string fileName, string tableName)
        {
            bool parsed = false;
            string selectQuery = $"select FileName from {tableName} where FileName='{fileName}';";
            OdbcCommand command = new OdbcCommand(selectQuery, _odbcConnection);
            _odbcConnection.Open();
            OdbcDataReader parsedFilesReader = command.ExecuteReader();
            while (parsedFilesReader.Read())
            {
                parsed = true;
                break;
            }
            parsedFilesReader.Close();
            _odbcConnection.Close();
            return parsed;

        }

    }
}
