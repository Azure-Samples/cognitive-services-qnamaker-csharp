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
            File.WriteAllText(outputFile, $"Line\tKbId\tQuery\tAnswer\tScore\tMetadata\tExpectedAnswer\tLabel{Environment.NewLine}");
            foreach (var queryData in inputQueryData)
            {
                try
                {
                    lineNumber++;
                    var (queryDto, kbId, expectedAnswer) = GetQueryDTO(queryData);
                    var response = runtimeClient.Runtime.GenerateAnswer(kbId, queryDto);

                    var resultLine = new List<string>();
                    resultLine.Add(lineNumber.ToString());
                    resultLine.Add(kbId);
                    resultLine.Add(queryDto.Question);

                    // Add the first answer and its score
                    var firstResult = response.Answers.FirstOrDefault();
                    var answer = firstResult?.Answer?.Replace("\n", "\\n");

                    resultLine.Add(answer);
                    resultLine.Add(firstResult?.Score?.ToString());

                    // Add Metadata
                    var metaDataList =  firstResult?.Metadata?.Select(x => $"{x.Name}:{x.Value}")?.ToList();
                    resultLine.Add(metaDataList == null ? string.Empty : string.Join("|", metaDataList));

                    // Add expected answer and label
                    if (!string.IsNullOrWhiteSpace(expectedAnswer))
                    {
                        resultLine.Add(expectedAnswer);
                        resultLine.Add(answer == expectedAnswer ? "Correct" : "Incorrect");
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

        private static (QueryDTO, string, string) GetQueryDTO(List<string> queryData)
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
                var md = queryData[2].Split('|').Select(x => new { kv = x.Split(':') }).Select(x => new MetadataDTO { Name = x.kv[0].Trim(), Value = x.kv[1].Trim() }).ToList();
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

            // expected answer - Optional
            string expectedAnswer = null;
            if (queryData.Count > 4 && !string.IsNullOrWhiteSpace(queryData[4]))
            {
                expectedAnswer = queryData[4];
            }

            return (queryDto, kbId, expectedAnswer);
        }

        private static List<string> GetTsvData(string line)
        {
            return line.Split('\t').ToList();
        }
    }
}
