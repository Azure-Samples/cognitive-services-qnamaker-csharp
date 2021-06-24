using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// NOTE: Install the Newtonsoft.Json NuGet package.
using Newtonsoft.Json;

namespace QnaMakerQuickstart
{
    public class KBDetails
    {
        public string id;
        public string hostName;
        public string lastAccessedTimestamp;
        public string lastChangedTimestamp;
        public string lastPublishedTimestamp;
        public string name;
        public string userId;
        public string[] urls;
        public string[] sources;
    }

    class Program
    {
        // Represents the various elements used to create HTTP request URIs
        // for QnA Maker operations.
        static string host = "https://<your-resource-name>.api.cognitive.microsoft.com";

        // Management APIs postpend the version to the route
        static string service = "/qnamaker/v4.0";

        // Management route - postpend when KBID is known
        static string baseRoute = "/knowledgebases";

        // Answer API does not use version in route
        static string endpointService = "/qnamaker";

        // Answer route
        static string generateAnswerRoute = "/generateAnswer";

        // NOTE: Replace this value with a valid QnA Maker subscription key found in 
        // Azure portal for QnA Maker resource.
        static string key = "<your-resource-key>";

        // NOTE: The KB ID is found in GetStatus() call
        static string kbid = "";

        // NOTE: The endpoint key is found in GetEndpointKeys() call
        static string endpoint_key = "";

        // NOTE: The host name is found in GetKnowledgeBaseHostNameDetails() call
        static string endpoint_host = "";

        // NOTE: KB model defintion
        static string kb_model = @"
{
  'name': 'QnA Maker FAQ from quickstart',
  'qnaList': [
    {
      'id': 0,
      'answer': 'You can use our REST APIs to manage your knowledge base. See here for details: https://westus.dev.cognitive.microsoft.com/docs/services/58994a073d9e04097c7ba6fe/operations/58994a073d9e041ad42d9baa',
      'source': 'Custom Editorial',
      'questions': [
        'How do I programmatically update my knowledge base?'
      ],
      'metadata': [
        {
          'name': 'category',
          'value': 'api'
        }
      ]
    }
  ],
  'urls': [
    'https://docs.microsoft.com/en-in/azure/cognitive-services/qnamaker/faqs',
    'https://docs.microsoft.com/en-us/bot-framework/resources-bot-framework-faq'
  ],
  'files': []
}
";

        public struct Response
        {
            public HttpResponseHeaders headers;
            public string response;

            public Response(HttpResponseHeaders headers, string response)
            {
                this.headers = headers;
                this.response = response;
            }
        }

        // Diplay JSON in readable format
        static string PrettyPrint(string s)
        {
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(s), Formatting.Indented);
        }

        // Generic GET method for QnA Maker
        // Used to:
        //      get operation status
        //      get KB details
        //      get KB endpoints
        async static Task<Response> Get(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new Response(response.Headers, responseBody);
            }
        }
        // Create KB POST method, passes model definition in request body
        async static Task<Response> PostCreateKB(string uri, string kb_model_definition)
        {
            Console.WriteLine("Create KB " + uri + ".");

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(kb_model_definition, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new Response(response.Headers, responseBody);
            }
        }
        // Get Create operation status
        async static Task<Response> GetStatus(string operationID)
        {
            // Builds the HTTP request URI.
            string uri = host + service + operationID;

            Console.WriteLine("Get Status " + uri + ".");

            return await Get(uri);
        }
        // Create KB is the process of sending the request
        // then polling to verify success or failure of the request
        // 
        // Polling is not request with the same URI
        // When the operation completes, the KB ID is returned
        // in the operation status details.
        async static Task CreateKB()
        {
            try
            {
                // Builds the HTTP request URI.
                string uri = host + service + baseRoute + "/create";

                // Starts the QnA Maker operation to create the knowledge base.
                var response = await PostCreateKB(uri, kb_model);

                // Retrieves the operation ID, so the operation's status can be
                // checked periodically.
                var operation = response.headers.GetValues("Location").First();

                Console.WriteLine("OperationID " + operation);

                // Displays the JSON in the HTTP response returned by the 
                // PostCreateKB(string) method.
                Console.WriteLine(PrettyPrint(response.response));

                // Iteratively gets the state of the operation creating the
                // knowledge base. Once the operation state is set to something other
                // than "Running" or "NotStarted", the loop ends.
                var done = false;
                while (true != done)
                {
                    // Gets the status of the operation.
                    response = await GetStatus(operation);

                    // Displays the JSON in the HTTP response returned by the
                    // GetStatus(string) method.
                    Console.WriteLine(PrettyPrint(response.response));

                    // Deserialize the JSON into key-value pairs, to retrieve the
                    // state of the operation.
                    var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.response);

                    // Gets and checks the state of the operation.
                    String state = fields["operationState"];

                    Console.WriteLine("Operation State " + state);

                    if (state.CompareTo("Running") == 0 || state.CompareTo("NotStarted") == 0)
                    {
                        // QnA Maker is still creating the knowledge base. The thread is 
                        // paused for a number of seconds equal to the Retry-After header value,
                        // and then the loop continues.
                        var wait = response.headers.GetValues("Retry-After").First();
                        Console.WriteLine("Waiting " + wait + " seconds...");
                        Thread.Sleep(Int32.Parse(wait) * 1000);
                    }
                    else
                    {
                        // QnA Maker has completed creating the knowledge base. 
                        // Get the kb ID from the resourceLocation header
                        // resourceLocation returns the full route to the KB
                        // remote the repetitive route to get the kb ID
                        kbid = fields["resourceLocation"].Replace("/knowledgebases/","");
                        Console.WriteLine("KB ID " + kbid);
                        done = true;
                    }
                }
            }
            catch(Exception ex)
            {
                // An error occurred while creating the knowledge base. Ensure that
                // you included your QnA Maker subscription key where directed in the sample.
                Console.WriteLine("An error occurred while creating the knowledge base.");
            }
        }
        // Publish KB, only status is returned. To determine the endpoint and host name
        // to pass a question to the KB, you must use other APIs
        //      Get knowledge base details -- host name
        //      Get endpoint
        async static Task PublishKB()
        {
            string responseText;

            var uri = host + service + baseRoute + "/" + kbid;
            Console.WriteLine("Publish KB " + uri + ".");

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
        }
        // Get KB details contains the host name
        async static Task GetKnowledgeBaseHostNameDetails()
        {
            var uri = host + service + baseRoute + "/" + kbid;

            Console.WriteLine("Get KB details " + uri + ".");

            var response = await Get(uri);

            // the endpoint key is the same endpoint key for all KBs associated with this QnA Maker service key
            var details = JsonConvert.DeserializeObject<KBDetails>(response.response);
            endpoint_host = details.hostName;
            Console.WriteLine("Endpoint host name " + endpoint_host + ".");
        }
        // Get KB endpoint keys returns authorization key required to pass a question
        async static Task GetEndpointKeys()
        {
            // notice that this uri doesn't change based on KB ID
            var uri = host + service + "/endpointkeys";

            Console.WriteLine("Get KB endpoints " + uri + ".");

            var response = await Get(uri);

            // the endpoint key is the same endpoint key for all KBs associated with this QnA Maker service key
            var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.response);
            endpoint_key = fields["primaryEndpointKey"];
            Console.WriteLine("Primary endpoint " + endpoint_key + ".");

        }
        // Pass question and get top answer
        async static Task GetAnswer(string question)
        {

            var uri = endpoint_host + endpointService + baseRoute + "/" + kbid + "/generateAnswer";

            Console.WriteLine("Get answers " + uri + ".");

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent("{question:'" + question + "'}", Encoding.UTF8, "application/json");

                // NOTE: The value of the header contains the string/text 'EndpointKey ' with the trailing space
                request.Headers.Add("Authorization", "EndpointKey " + endpoint_key);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(PrettyPrint(responseBody));

            }

            
        }
        static void Main(string[] args)
        {
            // Invoke the CreateKB() method to create a knowledge base, periodically 
            // checking the status of the QnA Maker operation until the 
            // knowledge base is created.
            CreateKB().Wait();

            // Publish KB. This doesn't return a polling location so there is no
            // need to check status. 
            PublishKB().Wait();

            // Get endpoint host name
            GetKnowledgeBaseHostNameDetails().Wait();

            GetEndpointKeys().Wait();

            // Get the top answer to a question
            GetAnswer("What languages does QnA Maker support?").Wait();

            // The console waits for a key to be pressed before closing.
            Console.ReadLine();
        }
    }
}
