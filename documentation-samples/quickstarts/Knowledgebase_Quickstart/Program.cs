/*
 * Tasks Included
 * 1. Create a knowledgebase
 * 2. Update a knowledgebase
 * 3. Publish a knowledgebase
 * 4. Download a knowledgebase
 * 5. Delete a knowledgebase
 */
namespace Knowledgebase_Quickstart
{
    /* To run this sample:
     * 1. Install the Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker NuGet package.
     * 2. Make sure you have updated all NuGet packages for your solution to the latest versions.
     * 3. Set/export the QNAMAKER_SUBSCRIPTION_KEY and QNAMAKER_REGION environment variables.
     */
    // <Dependencies>
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    // </Dependencies>

    class Program
    {
        // <Main>
        static void Main(string[] args)
        {
            // <Authorization>
            var subscriptionKey = Environment.GetEnvironmentVariable("QNAMAKER_SUBSCRIPTION_KEY");

            var region = Environment.GetEnvironmentVariable("QNAMAKER_REGION");

            var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = "https://" + region + ".api.cognitive.microsoft.com" };
            // </Authorization>

            // <EndpointKey>
            var endpointhostName = Environment.GetEnvironmentVariable("QNAMAKER_ENDPOINT_HOSTNAME");
            var endpointKey = Environment.GetEnvironmentVariable("QNAMAKER_ENDPOINT_KEY");
            var runtimeClient = new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = $"https://{endpointhostName}.azurewebsites.net" };
            // </EndpointKey>

            // Create a KB
            Console.WriteLine("Creating KB...");
            var kbId = CreateSampleKb(client).Result;
            Console.WriteLine("Created KB with ID : {0}", kbId);

            // Update the KB
            Console.WriteLine("Updating KB...");
            UpdateKB(client, kbId).Wait();
            Console.WriteLine("KB Updated.");

            // <PublishKB>
            Console.Write("Publishing KB...");
            client.Knowledgebase.PublishAsync(kbId).Wait();
            Console.WriteLine("KB Published.");
            // </PublishKB>

            // <DownloadKB>
            Console.Write("Downloading KB...");
            var kbData = client.Knowledgebase.DownloadAsync(kbId, EnvironmentType.Prod).Result;
            Console.WriteLine("KB Downloaded. It has {0} QnAs.", kbData.QnaDocuments.Count);
            // </DownloadKB>


            // <GenerateAnswer>
            Console.Write("Querying Endpoint...");
            var response = runtimeClient.Runtime.GenerateAnswerAsync(kbId, new QueryDTO { Question = "How do I manage my knowledgebase?" }).Result;
            Console.WriteLine("Endpoint Response: {0}.", response.Answers[0].Answer);
            // </GenerateAnswer>

            // <DeleteKB>
            Console.Write("Deleting KB...");
            client.Knowledgebase.DeleteAsync(kbId).Wait();
            Console.WriteLine("KB Deleted.");            
            // </DeleteKB>

            Console.WriteLine("Press any key to finish.");
            Console.Read();
        }
        // </Main>

        // <UpdateKB>
        private static async Task UpdateKB(IQnAMakerClient client, string kbId)
        {
            // Update kb
            var updateOp = await client.Knowledgebase.UpdateAsync(kbId, new UpdateKbOperationDTO
            {
                // Create JSON of changes 
                Add = new UpdateKbOperationDTOAdd { QnaList = new List<QnADTO> { new QnADTO { Questions = new List<string> { "bye" }, Answer = "goodbye" } } }, 
                Update = null,
                Delete = null
            });

            // Loop while operation is success
            updateOp = await MonitorOperation(client, updateOp);
        }
        // </UpdateKB>

        // <CreateKB>
        private static async Task<string> CreateSampleKb(IQnAMakerClient client)
        {
            var qna1 = new QnADTO
            {
                Answer = "You can use our REST APIs to manage your knowledge base.",
                Questions = new List<string> { "How do I manage my knowledgebase?" },
                Metadata = new List<MetadataDTO> { new MetadataDTO { Name = "Category", Value = "api" } }    
            };
            
            var file1 = new FileDTO
            {
                FileName="myFileName",
                FileUri="https://mydomain/myfile.md"

            };

            var urls = new List<string> {
                "https://docs.microsoft.com/en-in/azure/cognitive-services/qnamaker/faqs"
            };

            var createKbDto = new CreateKbDTO
            {
                Name = "QnA Maker FAQ from c# quickstart",
                QnaList = new List<QnADTO> { qna1 },
                //Files = new List<FileDTO> { file1 },
                Urls = urls

            };

            var createOp = await client.Knowledgebase.CreateAsync(createKbDto);
            createOp = await MonitorOperation(client, createOp);

            return createOp.ResourceLocation.Replace("/knowledgebases/", string.Empty);
        }
        // </CreateKB>

        // <MonitorOperation>
        private static async Task<Operation> MonitorOperation(IQnAMakerClient client, Operation operation)
        {
            // Loop while operation is success
            for (int i = 0;
                i < 20 && (operation.OperationState == OperationStateType.NotStarted || operation.OperationState == OperationStateType.Running);
                i++)
            {
                Console.WriteLine("Waiting for operation: {0} to complete.", operation.OperationId);
                await Task.Delay(5000);
                operation = await client.Operations.GetDetailsAsync(operation.OperationId);
            }

            if (operation.OperationState != OperationStateType.Succeeded)
            {
                throw new Exception($"Operation {operation.OperationId} failed to complete.");
            }
            return operation;
        }
        // </MonitorOperation>        
    }
}
