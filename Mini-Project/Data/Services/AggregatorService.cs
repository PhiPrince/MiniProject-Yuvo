using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mini_Project.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Data.Services
{
    public class AggregatorService
    {
        private OdbcConnection _odbcConnection;
        public IConfiguration Configuration { get; }
        private string rawTable;
        public AggregatorService(IConfiguration configuration)
        {
            Configuration = configuration;
            var ConnectionString = Configuration.GetConnectionString("VerticaConnectionString");
            rawTable = Configuration.GetValue<string>("RawTable");
            _odbcConnection = new OdbcConnection(ConnectionString);
        }

        public List<string> checkNewlyInsertedDataNotAggregated(string aggregationTimeFrame)
        {
            List<string> datesNotAggregated = new List<string>();
            List<string> loadedFilesDates = new List<string>();
            List<string> aggregatedDates = new List<string>();
            try
            {
                string selectQueryAggregatedDates = $"select * FROM TRANS_MW_AGG_SLOT_AGGREGATED where Description ='{aggregationTimeFrame}' order by AGGREGATED_DATES;";
                string selectQueryLoadedFiles = "select Date from loaded_files order by Date;";

                OdbcCommand command = new OdbcCommand(selectQueryAggregatedDates, _odbcConnection);
                OdbcCommand secondCommand = new OdbcCommand(selectQueryLoadedFiles, _odbcConnection);
                OdbcDataReader aggregatedDatesReader = command.ExecuteReader();
                OdbcDataReader loadedFilesDatesReader = secondCommand.ExecuteReader();

                int index = 0, j=0;

                while(loadedFilesDatesReader.Read())//save all loaded files from database 
                {
                    loadedFilesDates.Add(loadedFilesDatesReader[index].ToString().Replace("{", "").Replace("}", ""));
                    index++;
                }
                loadedFilesDatesReader.Close();

                while (aggregatedDatesReader.Read())//save all aggregated dates from database
                {
                    aggregatedDates.Add(aggregatedDatesReader[j].ToString().Replace("{", "").Replace("}", ""));
                    j++;
                }
                aggregatedDatesReader.Close();

                for (int i = 0; i < loadedFilesDates.Count; i++)//check files that aren't aggregated
                {
                    if (!aggregatedDates.Contains(loadedFilesDates[i]))
                    {
                        datesNotAggregated.Add(loadedFilesDates[i]);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return datesNotAggregated;
        }

        public void AggregateData()
        {
            Dictionary<string, string> aggregation = new Dictionary<string, string>()
                {
                    {"TRANS_MW_AGG_SLOT_HOURLY","hour"},
                    {"TRANS_MW_AGG_SLOT_DAILY","day"}
                };
            try
            {
                _odbcConnection.Open();
                foreach (KeyValuePair<string, string> entry in aggregation)//check if hours not aggregated what happens with daily aggregation
                {
                    List<string> datesNotAggregated = checkNewlyInsertedDataNotAggregated(entry.Value);
                    if (datesNotAggregated.Count != 0)
                    {
                        try
                        {
                            string table = rawTable,maxRxLevel="MAXRXLEVEL",maxTxLevel ="MAXTXLEVEL";

                            if(entry.Value=="day")//if aggregation per day set parameters to aggregate from hourly table
                            {
                                table = aggregation.FirstOrDefault(s => s.Value == "hour").Key;
                                maxRxLevel ="MAX_RX_LEVEL";
                                maxTxLevel = "MAX_TX_LEVEL";
                            }

                            string aggregationQuery = 
                                     $"insert into {entry.Key}(Time, LINK, SLOT,NeType,NeAlias, MAX_RX_LEVEL, MAX_TX_LEVEL, RSL_DEVIATION)" +
                                     $"select date_trunc('{entry.Value}', Time), LINK, SLOT,NeType,NeAlias, max({maxRxLevel}), max({maxTxLevel}), abs(max({maxRxLevel})) - abs(max({maxTxLevel}))" +
                                     $"from {table} " +
                                     $"where date_trunc('{entry.Value}',Time) not in (select date_trunc('{entry.Value}',Time) from {entry.Key})" +
                                     $" group by 1,2,3,4,5; ";
                            string newDatesAggregatedQueries = "";
                            foreach (var date in datesNotAggregated)//save files that have been aggregated
                            {
                                newDatesAggregatedQueries += $"insert into TRANS_MW_AGG_SLOT_AGGREGATED values('{date}','{entry.Value}');";
                            }
                            string queriesToExecute = aggregationQuery + newDatesAggregatedQueries;
                            _odbcConnection.Execute(queriesToExecute);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No new data to aggregate.");
                    }
                }
                _odbcConnection.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}