using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mini_Project.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project.Data.Services
{
    public class ParserService
    {
        public IConfiguration Configuration { get; }
        private string filePath, parsedFiles, filesToLoadPath;
        private Helper _helper;
        private OdbcConnection _odbcConnection;
        public ParserService(IConfiguration configuration)
        {
            Configuration = configuration;

            filePath = Configuration.GetValue<string>("FilesToBeParsed");
            filesToLoadPath = Configuration.GetValue<string>("FilesToBeLoaded");
            parsedFiles = Configuration.GetValue<string>("ParsedFiles");
            var ConnectionString = Configuration.GetConnectionString("VerticaConnectionString");

            _helper = new Helper(configuration);
            _odbcConnection = new OdbcConnection(ConnectionString);
        }
        public int getNetworkSID(string toHash)//get network SID using hashing function
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(toHash));
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }
        public string getLink(List<string> Columns,Dictionary<string,int> headers)//parse link column
        {
            var semiParsedValue = Columns[headers["Object"]].Split("_")[0];
            if (semiParsedValue.Contains(".") && semiParsedValue.Contains("+"))
            {
                var dotValues = semiParsedValue.Split("/")[1].Split("+");
                string slotPart = "";
                foreach (var item in dotValues)
                {
                    slotPart += item.Split(".").First() + "+";
                }
                var portPart = dotValues[0].Split(".").Last();
                slotPart.Remove(slotPart.Length - 1);
                return slotPart + "/" + portPart;
            }
            else if (semiParsedValue.Contains("."))
            {
                var dottedValues = semiParsedValue.Split("/")[1].Split(".");
                return dottedValues.First() + "/" + dottedValues.Last();
            }
            else
            {
                return semiParsedValue.Split("/")[1] + "/" + semiParsedValue.Split("/").Last();
            }
        }
        public List<string> GetColumns(string file, string line,Dictionary<string,int> headers,string DateTimeKey)//add necessary columns to work with
        {
            List<string> Columns = new List<string>();
            Columns = line.Split(",").ToList();
            int hashedNetworkSID = getNetworkSID(Columns[headers["Object"]-2] + Columns[headers["NeAlias"]-2]);
            Columns.Insert(0, hashedNetworkSID.ToString());
            Columns.Insert(1, DateTimeKey);
            List<string> GeneretadColumns = new List<string> { "LINK", "TID", "FARENDTID", "SLOT", "PORT" };
            Columns.InsertRange(Columns.Count, GeneretadColumns);
            return Columns;
        }
        public Dictionary<string, int> FillDictionary(string file)// fill dictionary of headers with indexes
        {
            string headers = File.ReadLines(file).First();
            List<string> GeneretadHeaders = new List<string> { "LINK", "TID", "FARENDTID", "SLOT", "PORT" };
            Dictionary<string, int> headersDict = new Dictionary<string, int>();
            headersDict.Add("NetworkSID", 0);
            headersDict.Add("DATETIME_KEY", 1);
            int headerIndex = 2;
            foreach (string header in headers.Split(','))
            {
                headersDict.Add(header, headerIndex);
                headerIndex++;
            }
            foreach (string header in GeneretadHeaders)
            {
                headersDict.Add(header, headerIndex);
                headerIndex++;
            }
            return headersDict;
        }
        public string addNewRecordCondition(string parsedRow, string linkColumn, Dictionary<string,int> headers,int index)//add a new record if contains + in link
        {
            string secondRecord = "";
                for (int i = 0; i < parsedRow.Split(',').Length; i++)
                {
                    if (i != parsedRow.Split(',').Length - 3)
                    {
                        secondRecord += parsedRow.Split(',')[i] + ",";
                    }
                    else
                    {
                    secondRecord += linkColumn.Split("/")[0].Split("+")[index] + ",";
                    }
                }
            return secondRecord.Remove(parsedRow.Length-1);
        }
        public bool insertFileNameToDatabase(string fileName,string fileDate)
        {
            try
            {
                _odbcConnection.Open();
                _odbcConnection.Execute($"insert into parsed_files values('{Path.GetFileName(fileName)}','{fileDate}');");
                _odbcConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void ParseFiles()
        {
            string[] files = Directory.GetFiles(filePath);
            foreach (string file in files)
            {
                if(_helper.checkForPresentFiles(Path.GetFileName(file),"parsed_files"))
                {
                    continue;
                }
                Dictionary<string, int> headers = FillDictionary(file);
                List<string> parsedRows = new List<string>();
                List<string> lines = new List<string>();

                int lineNumber = 0;
                string DateTimeKey = _helper.getDateFromFileName(file);

                using (StreamReader sr = File.OpenText(file))
                {
                    string line = String.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (!string.IsNullOrEmpty(line))
                        {
                            string parsedRow = "";
                            List<string> Columns = new List<string>();
                            Columns = GetColumns(file, line, headers, DateTimeKey);
                            if (Columns[headers["Object"]] == "Unreachable Bulk FC" || Columns[headers["FailureDescription"]] != "-")
                            {
                                continue;
                            }
                            string linkColumn = "";
                            try
                            {
                                for (int h = 0; h < Columns.Count; h++)
                                {
                                    if (h == headers["NodeName"] || h == headers["Position"] || h == headers["IdLogNum"])//removed disabled columns
                                    {
                                        continue;
                                    }
                                    string parsedValue = "";
                                    string columnName = Columns[h];
                                    switch (columnName)
                                    {
                                        case "LINK":
                                            linkColumn = getLink(Columns, headers);
                                            parsedValue = linkColumn;
                                            break;
                                        case "TID":
                                            string[] temp = Columns[headers["Object"]].Split("_");
                                            for (int i = 1; i < temp.Length - 2; i++)
                                            {
                                                parsedValue += temp[i];
                                            }
                                            break;
                                        case "FARENDTID":
                                            parsedValue = Columns[headers["Object"]].Split("_").Last();
                                            break;
                                        case "SLOT":
                                            if (linkColumn.Contains("+"))
                                            {
                                                parsedValue += linkColumn.Split("/")[0].Split("+").First();
                                            }
                                            else
                                            {
                                                parsedValue += linkColumn.Split("/")[0];
                                            }
                                            break;
                                        case "PORT":
                                            if (linkColumn.Contains("+"))
                                            {
                                                parsedValue += "1";
                                            }
                                            else
                                            {
                                                parsedValue += linkColumn.Split("/")[1];
                                            }
                                            break;
                                        default:
                                            parsedValue = columnName;
                                            break;
                                    }
                                    parsedRow += parsedValue + ",";
                                    if (columnName == "PORT" && linkColumn.Contains("+"))
                                    {
                                        for (int j=1;j< linkColumn.Split("/")[0].Split("+").Count();j++)
                                        {
                                            parsedRows.Add(addNewRecordCondition(parsedRow, linkColumn, headers,j));
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }

                            if (parsedRow != "")
                            {
                                parsedRows.Add(parsedRow.Remove(parsedRow.Length - 1));
                            }
                        }
                    }
                }

                try
                {
                    if(insertFileNameToDatabase(file,DateTimeKey))
                    { 
                        Console.WriteLine("File name saved in database successfuly!");
                    }
                    else
                    {
                        Console.WriteLine("Couldn't insert Parsed file name in database");
                    }
                    File.AppendAllLines(filesToLoadPath + @"\" + Path.GetFileNameWithoutExtension(file) + ".csv", parsedRows);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                string newDestination = parsedFiles + @"\" + Path.GetFileName(file);
                File.Move(file, newDestination);
            }
        }
    }
}

