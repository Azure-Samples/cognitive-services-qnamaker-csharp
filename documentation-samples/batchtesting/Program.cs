using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace batchtesting
{
    class Program
    {
        private const int DefaultTopValue = 3;

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                var exeName = "batchtesting.exe";
                Console.WriteLine($"Usage: {exeName} <tsv-inputfile> <runtime-hostname> <runtime-endpointkey> <tsv-outputfile>");
                Console.WriteLine($"{exeName} input.tsv https://myhostname.azurewebsites.net 5397A838-2B74-4E55-8111-D60ED1D7CF7F output.tsv");
                Console.WriteLine();

                return;
            }

            var i = 0;
            var inputFile = args[i++];
            var runtimeHost = args[i++];
            var endpointKey = args[i++];
            var outputFile = args[i++];

            var inputQueries = File.ReadAllLines(inputFile);
            var inputQueryData = inputQueries.Select(x => GetTsvData(x)).ToList();
            var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = runtimeHost };

            var lineNumber = 0;
            File.WriteAllText(outputFile, $"Line\tKbId\tQuery\tAnswer1\tScore1{Environment.NewLine}");
            foreach (var queryData in inputQueryData)
            {
                try
                {
                    lineNumber++;
                    var (queryDto, kbId) = GetQueryDTO(queryData);
                    var response = runtimeClient.Runtime.GenerateAnswer(kbId, queryDto);

                    var resultLine = new List<string>();
                    resultLine.Add(lineNumber.ToString());
                    resultLine.Add(kbId);
                    resultLine.Add(queryDto.Question);
                    foreach(var answer in response.Answers)
                    {
                        resultLine.Add(answer.Answer);
                        resultLine.Add(answer.Score.ToString());
                    }

                    var result = string.Join('\t', resultLine);
                    File.AppendAllText(outputFile, $"{result}{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing line : {lineNumber}, {ex}");
                }
            }

        }

        private static (QueryDTO, string) GetQueryDTO(List<string> queryData)
        {
            var queryDto = new QueryDTO();

            // Process input tsv line
            // KbId - not Optional
            var kbId = queryData[0];

            // Question - not Optional
            queryDto.Question = queryData[1];

            // Metadata - Optional
            if (queryData.Count > 2 && !string.IsNullOrWhiteSpace(queryData[2]))
            {
                var md = queryData[2].Split('|').Select(x => new { kv = x.Split(':') }).Select(x => new MetadataDTO { Name = x.kv[0], Value = x.kv[1] }).ToList();
                queryDto.StrictFilters = md;
            }

            // top - Optional
            if (queryData.Count > 3 && !string.IsNullOrWhiteSpace(queryData[3]) && int.TryParse(queryData[3], out var top))
            {
                queryDto.Top = top;
            }
            else
            {
                queryDto.Top = DefaultTopValue;
            }


            return (queryDto, kbId);
        }

        private static List<string> GetTsvData(string line)
        {
            return line.Split('\t').ToList();
        }
    }
}
