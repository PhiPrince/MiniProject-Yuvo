using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mini_Project.Controllers;

namespace Mini_Project.Data.Services
{
    public class LoaderService
    {
        private OdbcConnection _odbcConnection;
        public IConfiguration Configuration{ get; }
        private string loadedFiles,verticaLogFile,toBeLoadedFilesPath,rawTable;
        private Helper _helper;
        public LoaderService(IConfiguration configuration)
        {
            Configuration = configuration;
            _helper = new Helper(configuration);

            var ConnectionString = Configuration.GetConnectionString("VerticaConnectionString");
            loadedFiles = Configuration.GetValue<string>("LoadedFiles");
            verticaLogFile = Configuration.GetValue<string>("VerticaLogFile");
            toBeLoadedFilesPath = Configuration.GetValue<string>("FilesToBeLoaded");
            rawTable = Configuration.GetValue<string>("RawTable");

            _odbcConnection = new OdbcConnection(ConnectionString);
        }
        public void CopyToDatabase()
        {
            string[] files = Directory.GetFiles(toBeLoadedFilesPath);
            foreach (string file in files)
            {
                if (_helper.checkForPresentFiles(Path.GetFileName(file), "loaded_files"))
                {
                    continue;
                }
                string filename = Path.GetFileName(file);
                try
                {
                    _odbcConnection.Open();
                    string query = $"copy {rawTable} from local '{file}' delimiter ',';" +
                                    //"DIRECT REJECTED DATA '{verticaLogFile}' EXCEPTIONS '{verticaLogFile}';"+
                                    $"insert into loaded_files values('{filename}','{_helper.getDateFromFileName(file)}');";
                    _odbcConnection.Execute(query);
                    File.Move(file, loadedFiles + @"\" + "LoadedIn_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + filename);
                    _odbcConnection.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
