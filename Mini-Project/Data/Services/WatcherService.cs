using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mini_Project.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Data.Services
{
    public class WatcherService
    {
        private FileSystemWatcher parsedFilesWatcher,toBeLoadedFilesWatcher,toBeAggregatedFilesWatcher;
        public IConfiguration Configuration { get; }
        private string toBeparsedFilesPath,toBeLoadedFilesPath,toBeAggregatedFilesPath;
        private ILogger<ParserController> parserLogger;
        private ILogger<LoaderController> loaderLogger;
        private ILogger<AggregatorController> aggregatorLogger;
        public WatcherService(IConfiguration configuration)
        {
            Configuration = configuration;

            toBeparsedFilesPath = Configuration.GetValue<string>("FilesToBeParsed");
            parsedFilesWatcher = new FileSystemWatcher(toBeparsedFilesPath);
            parsedFilesWatcher.EnableRaisingEvents = true;
            parsedFilesWatcher.Created += new FileSystemEventHandler(FileCreatedToBeParsed);


            toBeLoadedFilesPath = Configuration.GetValue<string>("FilesToBeLoaded");
            toBeLoadedFilesWatcher = new FileSystemWatcher(toBeLoadedFilesPath);
            toBeLoadedFilesWatcher.EnableRaisingEvents = true;
            toBeLoadedFilesWatcher.Created += new FileSystemEventHandler(FileCreatedToBeLoaded);


            toBeAggregatedFilesPath= Configuration.GetValue<string>("LoadedFiles");
            toBeAggregatedFilesWatcher= new FileSystemWatcher(toBeAggregatedFilesPath);
            toBeAggregatedFilesWatcher.EnableRaisingEvents = true;
            toBeAggregatedFilesWatcher.Created += new FileSystemEventHandler(FileCreatedToBeAggregated);

            checkIfFileAlreadyExist(toBeparsedFilesPath);
        }
        public void checkIfFileAlreadyExist(string parseFilesPath)//3al kel lezim
        {
            if(Directory.GetFiles(parseFilesPath).Length!=0)
            {
                ParserService parserService = new ParserService(Configuration);
                parserService.ParseFiles();
            }
        }
        private void FileCreatedToBeParsed(Object sender, FileSystemEventArgs e) 
        {
            ParserService parserService = new ParserService(Configuration);
            parserService.ParseFiles();
        }
        private void FileCreatedToBeLoaded(Object sender, FileSystemEventArgs e)
        {
            LoaderService loaderService = new LoaderService(Configuration);
            loaderService.CopyToDatabase();
        }
        private void FileCreatedToBeAggregated(Object sender, FileSystemEventArgs e)
        {
            AggregatorService aggregatroService = new AggregatorService(Configuration);
            aggregatroService.AggregateData();
        }
        public void setLoggers(ILogger<ParserController> parserLogger, ILogger<LoaderController> loaderLogger,ILogger<AggregatorController> aggregatorLogger)
        {
            this.parserLogger = parserLogger;
            this.loaderLogger = loaderLogger;
            this.aggregatorLogger = aggregatorLogger;
        }
    }
}
