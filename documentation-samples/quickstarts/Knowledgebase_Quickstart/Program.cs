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

            var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = "https://westus.api.cognitive.microsoft.com" };
            // </Authorization>

            // Create a KB
            Console.WriteLine("Creating KB...");
            var kbId = CreateSampleKb(client).Result;
            Console.WriteLine("Created KB with ID : {0}", kbId);

            // Update the KB
            Console.WriteLine("Updating KB...");
            UpdateKB(client, kbId).Wait();
            Console.WriteLine("KB Updated.");

            // Publish the KB
            Console.Write("Publishing KB...");
            client.Knowledgebase.PublishAsync(kbId).Wait();
            Console.WriteLine("KB Published.");

            // <DownloadKB>
            Console.Write("Downloading KB...");
            var kbData = client.Knowledgebase.DownloadAsync(kbId, EnvironmentType.Prod).Result;
            Console.WriteLine("KB Downloaded. It has {0} QnAs.", kbData.QnaDocuments.Count);
            // </DownloadKB>

            // <DeleteKB>
            Console.Write("Deleting KB...");
            client.Knowledgebase.DeleteAsync(kbId).Wait();
            Console.WriteLine("KB Deleted.");            
            // </DeleteKB>
        }
        // </Main>

        // <UpdateKB>
        private static async Task UpdateKB(IQnAMakerClient client, string kbId)
        {
            /*
             * QnAMaker API - Update a knowledgebase
             */
            var updateOp = await client.Knowledgebase.UpdateAsync(kbId, new UpdateKbOperationDTO
            {
                Add = new UpdateKbOperationDTOAdd { QnaList = new List<QnADTO> { new QnADTO { Questions = new List<string> { "bye" }, Answer = "goodbye" } } }
            });

            // Loop while operation is success
            updateOp = await MonitorOperation(client, updateOp);

            // END - Update a knowledgebase
        }
        // </UpdateKB>

        // <CreateKB>
        private static async Task<string> CreateSampleKb(IQnAMakerClient client)
        {
            /*
             * QnAMaker API - Create a knowledgebase
             */
            var qna = new QnADTO
            {
                Answer = "You can use our REST APIs to manage your knowledge base.",
                Questions = new List<string> { "How do I manage my knowledgebase?" },
                Metadata = new List<MetadataDTO> { new MetadataDTO { Name = "Category", Value = "api" } }
            };

            var urls = new List<string> { "https://docs.microsoft.com/en-in/azure/cognitive-services/qnamaker/faqs" };
            var createKbDto = new CreateKbDTO
            {
                Name = "QnA Maker FAQ from quickstart",
                QnaList = new List<QnADTO> { qna },
                Urls = urls
            };

            var createOp = await client.Knowledgebase.CreateAsync(createKbDto);
            createOp = await MonitorOperation(client, createOp);

            // END - Create a knowledgebase

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
                throw new Exception($"Operation {operation.OperationId} failed to completed.");
            }
            return operation;
        }
        // </MonitorOperation>        
    }
}
