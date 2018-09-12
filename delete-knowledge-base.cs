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

namespace QnAMaker
{
	class Program
	{
		static string host = "https://westus.api.cognitive.microsoft.com";
		static string service = "/qnamaker/v4.0";
		static string method = "/knowledgebases/";

		// NOTE: Replace this with a valid subscription key from the Azure portal.
		static string key = "ADD KEY HERE";

		// NOTE: Replace this with a valid knowledge base ID, the one you want to delete, from qnamaker.ai.
		static string kb = "ADD KNOWLEDGE BASE ID HERE";

		static string PrettyPrint(string s)
		{
			return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(s), Formatting.Indented);
		}

		async static Task<string> Delete(string uri)
		{
			using (var client = new HttpClient())
			using (var request = new HttpRequestMessage())
			{
				request.Method = HttpMethod.Delete;
				request.RequestUri = new Uri(uri);
				request.Headers.Add("Ocp-Apim-Subscription-Key", key);

				var response = await client.SendAsync(request);
				if (response.IsSuccessStatusCode)
				{
					return "{'result' : 'Success.'}";
				}
				else
				{
					return await response.Content.ReadAsStringAsync();
				}
			}
		}

		async static void DeleteKB()
		{
			var uri = host + service + method + kb;
			Console.WriteLine("Calling " + uri + ".");
			var response = await Delete(uri);
			Console.WriteLine(PrettyPrint(response));
			Console.WriteLine("Press any key to continue.");
		}

		static void Main(string[] args)
		{
			DeleteKB();
			Console.ReadLine();
		}
	}
}