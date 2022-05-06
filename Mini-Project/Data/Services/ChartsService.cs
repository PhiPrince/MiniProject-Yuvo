using Microsoft.Extensions.Configuration;
using Mini_Project.Models;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Data.Services
{
    public class ChartsService
    {
        private IConfiguration Configuration { get; }
        private OdbcConnection _odbcConnection;
        public ChartsService(IConfiguration configuration)
        {
            Configuration = configuration;
            var ConnectionString = Configuration.GetConnectionString("VerticaConnectionString");
            _odbcConnection = new OdbcConnection(ConnectionString);
        }
        public List<AggregatedData> getValues(string timeframe,DateTime? dateFrom, DateTime? dateTo)
        {
            List<AggregatedData> data = new List<AggregatedData>();
            Dictionary<string, string> aggregation = new Dictionary<string, string>()
                {
                    {"hour","TRANS_MW_AGG_SLOT_HOURLY"},
                    {"day","TRANS_MW_AGG_SLOT_DAILY"}
                };


            _odbcConnection.Open();
            string selectColumns = @$" SELECT Time,LINK,NeType,NeAlias,max(MAX_RX_LEVEL),max(MAX_TX_LEVEL),max(RSL_DEVIATION)
                                                    FROM {aggregation[timeframe]} ";
            string selectQueryAggregatedDates = "";
            string groupBY = "GROUP BY 1,2,3,4;";
            string DateFrom ="",DateTo="";
            if (dateFrom != null && dateTo != null)
            {
                DateFrom = dateFrom.ToString();
                DateTo = dateTo.ToString();
                selectQueryAggregatedDates = selectColumns + $"where Time between '{DateFrom}' and '{DateTo}' " + groupBY;
            }
            else if (dateFrom != null)
            {
                DateFrom = dateFrom.ToString();
                selectQueryAggregatedDates = selectColumns + $"where Time >'{DateFrom}' " + groupBY;
            }
            else if (dateTo != null)
            {
                DateTo = dateTo.ToString();
                selectQueryAggregatedDates = selectColumns + $"where Time <'{DateTo}' " + groupBY;
            }
            else
            {
                selectQueryAggregatedDates = selectColumns + groupBY;
            }
            
            OdbcCommand command = new OdbcCommand(selectQueryAggregatedDates, _odbcConnection);
            OdbcDataReader aggregatedDatesReader = command.ExecuteReader();
            while (aggregatedDatesReader.Read())
            {
                AggregatedData aggregatedData = new AggregatedData();
                aggregatedData.Time = (DateTime)aggregatedDatesReader[0];
                aggregatedData.Link = aggregatedDatesReader[1].ToString();
                aggregatedData.NeType = aggregatedDatesReader[2].ToString();
                aggregatedData.NeAlias = aggregatedDatesReader[3].ToString();
                aggregatedData.MAX_RX_LEVEL = (double)aggregatedDatesReader[4];
                aggregatedData.MAX_TX_LEVEL = (double)aggregatedDatesReader[5];
                aggregatedData.RSL_DEVIATION = (double)aggregatedDatesReader[6];
                data.Add(aggregatedData);
            }
            aggregatedDatesReader.Close();
            _odbcConnection.Close();
            return data;
        }
    }
}
