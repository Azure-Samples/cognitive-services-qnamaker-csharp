
using System;
using System.Net.Http;

// NOTE: Install the Newtonsoft.Json NuGet package.
using Newtonsoft.Json;

namespace QnAMakerQuickstart
{
    class Program
    {
        static string host = "https://westus.api.cognitive.microsoft.com";
        static string service = "/qnamaker/v4.0";
        static string method = "/knowledgebases/";

        // NOTE: Replace this with a valid subscription key.
        static string key = "<qna-maker-subscription-key>";

        // NOTE: Replace this with a valid knowledge base ID.
        static string kb = "<qna-maker-knowledge-base-id>";

        static string PrettyPrint(string s)
        {
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(s), Formatting.Indented);
        }

        async static void PublishKB()
        {
            string responseText;

            var uri = host + service + method + kb;
            Console.WriteLine("Calling " + uri + ".");
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);

                // successful status doesn't return an JSON so create one
                if (response.IsSuccessStatusCode)
                {
                    responseText = "{'result' : 'Success.'}";
                }
                else
                {
                    responseText =  await response.Content.ReadAsStringAsync();
                }
            }
            Console.WriteLine(PrettyPrint(responseText));
            Console.WriteLine("Press any key to continue.");
        }

        static void Main(string[] args)
        {
            PublishKB();
            Console.ReadLine();
        }
    }
}