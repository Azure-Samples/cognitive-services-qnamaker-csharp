using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
using Azure.AI.Language.QuestionAnswering;
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
            if (args.Length < 4)
            {
                var exeName = "batchtesting.exe";
                Console.WriteLine("For Qna Maker GA");
                Console.WriteLine($"Usage: {exeName} <tsv-inputfile> <runtime-hostname> <runtime-endpointkey> <tsv-outputfile>");
                Console.WriteLine($"{exeName} input.tsv https://myhostname.azurewebsites.net 5397A838-2B74-4E55-8111-D60ED1D7CF7F output.tsv");
                Console.WriteLine("For QnA Maker managed (preview)");
                Console.WriteLine($"Usage: {exeName} <tsv-inputfile> <cs-hostname> <cs-endpointkey> <tsv-outputfile>");
                Console.WriteLine($"{exeName} input.tsv https://myhostname.cognitiveservices.azure.com b0863a25azsxdcf0b6855e9e988805ed output.tsv");
                Console.WriteLine($"Usage: {exeName} <tsv-inputfile> <cs-hostname> <cs-endpointkey> <tsv-outputfile>");
                Console.WriteLine($"{exeName} input.tsv https://myhostname.cognitiveservices.azure.com b0863a25azsxdcf0b6855e9e988805ed output.tsv language");
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
            IQnAMakerClient qnaMakerClient = null;
            QnAMakerRuntimeClient qnaMakerRuntimeClient = null;
            QuestionAnsweringClient questionAnsweringClient = null;


            var isLanguage = false;
            var isQnAMakerV2 = false;
            if (args.Length == 5 && string.Equals(args[4], "language", StringComparison.OrdinalIgnoreCase))
            {
                isLanguage = true;
            }

            if (isLanguage)
            {
                questionAnsweringClient = new QuestionAnsweringClient(new Uri(runtimeHost), new Azure.AzureKeyCredential(endpointKey));
            }
            else
            {
                isQnAMakerV2 = CheckForQnAMakerV2(runtimeHost);

                if (isQnAMakerV2)
                {
                    qnaMakerClient = GetQnAMakerClient(endpointKey, runtimeHost);
                }
                else
                {
                    qnaMakerRuntimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = runtimeHost };
                }
            }

            var lineNumber = 0;
            var answerSpanHeader = isQnAMakerV2 || isLanguage ? "\tAnswerSpanText\tAnswerSpanScore" : string.Empty;

            File.WriteAllText(outputFile, $"Line\tKbId\tQuery\tAnswer\tScore{answerSpanHeader}\tMetadata\tAnswerId\tExpectedAnswerId\tLabel{Environment.NewLine}");
            if (isLanguage)
            {
                QueryKnowledgebases(questionAnsweringClient, inputQueryData, outputFile);
            }
            else
            {
                var watch = new Stopwatch();
                watch.Start();
                var maxLines = inputQueryData.Count;
                foreach (var queryData in inputQueryData)
                {
                    try
                    {
                        lineNumber++;
                        var (queryDto, kbId, expectedAnswerId) = GetQueryDTO(queryData, isQnAMakerV2);

                        QnASearchResultList response = null;

                        if (isQnAMakerV2)
                        {
                            response = qnaMakerClient.Knowledgebase.GenerateAnswerAsync(kbId, queryDto).Result;
                        }
                        else
                        {
                            response = qnaMakerRuntimeClient.Runtime.GenerateAnswerAsync(kbId, queryDto).Result;
                        }

                        var resultLine = new List<string>();
                        resultLine.Add(lineNumber.ToString());
                        resultLine.Add(kbId);
                        resultLine.Add(queryDto.Question);

                        // Add the first answer and its score
                        var firstResult = response.Answers.FirstOrDefault();
                        var answer = firstResult?.Answer?.Replace("\n", "\\n");

                        resultLine.Add(answer);
                        resultLine.Add(firstResult?.Score?.ToString());

                        if (isQnAMakerV2 && firstResult?.AnswerSpan?.Text != null)
                        {
                            resultLine.Add(firstResult?.AnswerSpan?.Text);
                            resultLine.Add(firstResult?.AnswerSpan?.Score?.ToString());
                        }

                        // Add Metadata
                        var metaDataList = firstResult?.Metadata?.Select(x => $"{x.Name}:{x.Value}")?.ToList();
                        resultLine.Add(metaDataList == null ? string.Empty : string.Join("|", metaDataList));

                        // Add the QnaId
                        var firstQnaId = firstResult?.Id?.ToString();
                        resultLine.Add(firstQnaId);

                        // Add expected answer and label
                        if (!string.IsNullOrWhiteSpace(expectedAnswerId))
                        {
                            resultLine.Add(expectedAnswerId);
                            resultLine.Add(firstQnaId == expectedAnswerId ? "Correct" : "Incorrect");
                        }

                        var result = string.Join('\t', resultLine);
                        File.AppendAllText(outputFile, $"{result}{Environment.NewLine}");
                        PrintProgress(watch, lineNumber, maxLines);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing line : {lineNumber}, {ex}");
                    }
                }
            }

        }

        private static (string, string, string) GetQueryKnowledgebasesRequest(List<string> queryData)
        {
            var (queryDto, projectName, expectedAnswerId) = GetQueryDTO(queryData, true);
            return (queryDto.Question, projectName, expectedAnswerId);
        }

        private static void QueryKnowledgebases(QuestionAnsweringClient questionAnsweringClient, List<List<string>> inputQueryData, string outputFileName)
        {

            var watch = new Stopwatch();
            watch.Start();


            var maxLines = inputQueryData.Count;
            var lineNumber = 0;
            foreach (var queryData in inputQueryData)
            {
                //var projectName = queryData.ElementAt(0);
                //var question = queryData.ElementAt(1);
                var (question, projectName, expectedAnswerId) = GetQueryKnowledgebasesRequest(queryData);

                try
                {
                    lineNumber++;
                    var answerResult = questionAnsweringClient.GetAnswers(question, new QuestionAnsweringProject(projectName, "production"));
                   

                    var resultLine = new List<string>();
                    resultLine.Add(lineNumber.ToString());
                    resultLine.Add(projectName);
                    resultLine.Add(question);

                    // Add the first answer and its score
                    var firstResult = answerResult.Value.Answers.FirstOrDefault();
                    var answer = firstResult?.Answer?.Replace("\n", "\\n");

                    resultLine.Add(answer);
                    resultLine.Add(firstResult?.Confidence?.ToString());

                    if (firstResult?.ShortAnswer?.Text != null)
                    {
                        resultLine.Add(firstResult?.ShortAnswer?.Text);
                        resultLine.Add(firstResult?.ShortAnswer?.Confidence?.ToString());
                    }

                    // Add Metadata
                    var metaDataList = firstResult?.Metadata?.Select(x => $"{x.Key}:{x.Value}")?.ToList();
                    resultLine.Add(metaDataList == null ? string.Empty : string.Join("|", metaDataList));

                    // Add the QnaId
                    var firstQnaId = firstResult?.QnaId?.ToString();
                    resultLine.Add(firstQnaId);

                    // Add expected answer and label
                    if (!string.IsNullOrWhiteSpace(expectedAnswerId))
                    {
                        resultLine.Add(expectedAnswerId);
                        resultLine.Add(firstQnaId == expectedAnswerId ? "Correct" : "Incorrect");
                    }

                    var result = string.Join('\t', resultLine);
                    File.AppendAllText(outputFileName, $"{result}{Environment.NewLine}");
                    PrintProgress(watch, lineNumber, maxLines);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing line : {lineNumber}, {ex}");
                }
            }
        }

        private static void PrintProgress(Stopwatch watch, int lineNumber, int maxLines)
        {
            var qps = (double)lineNumber / watch.ElapsedMilliseconds * 1000.0;
            var remaining = maxLines - lineNumber;
            var etasecs =  (long)Math.Ceiling(remaining * qps);
            Console.WriteLine($"Done : {lineNumber}/{maxLines}. {remaining} remaining. ETA: {etasecs} seconds.");
        }

        private static (QueryDTO, string, string) GetQueryDTO(List<string> queryData, bool isQnAMakerV2 = false)
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

            if (isQnAMakerV2)
            {
                queryDto.AnswerSpanRequest = new QueryDTOAnswerSpanRequest()
                {
                    Enable = true
                };
            }

            // expected answer - Optional
            string expectedAnswerId = null;
            if (queryData.Count > 4 && !string.IsNullOrWhiteSpace(queryData[4]))
            {
                expectedAnswerId = queryData[4];
            }

            return (queryDto, kbId, expectedAnswerId);
        }

        private static List<string> GetTsvData(string line)
        {
            return line.Split('\t').ToList();
        }

        private static bool CheckForQnAMakerV2(string endpointUrl)
        {
            if (!endpointUrl.Contains("azurewebsites.net")) return true;
            return false;
        }

        private static IQnAMakerClient GetQnAMakerClient(string qnaMakerSubscriptionKey, string endpointHost)
        {
            IQnAMakerClient client = new QnAMakerClient(new ApiKeyServiceClientCredentials(qnaMakerSubscriptionKey))
            {
                Endpoint = endpointHost
            };


            return client;
        }

    }
}
