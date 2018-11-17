using System;
using System.Net.Http;

namespace QnAMakerPublishQuickstart
{
    class Program
    {

        static void Main(string[] args)
        {
            string knowledge_base_id = "YOUR-KNOWLEDGE-BASE-ID";
            string resource_key = "YOUR-RESOURCE-KEY";

            string host = String.Format("https://westus.api.cognitive.microsoft.com/qnamaker/v4.0/knowledgebases/{0}", knowledge_base_id);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(host);
                request.Headers.Add("Ocp-Apim-Subscription-Key", resource_key);

                // Send request to Azure service, get response
                // returns 204 with no content
                var response = client.SendAsync(request).Result;

                Console.WriteLine("Press any key to continue.");
                Console.ReadLine();
            }
        }
    }
}
