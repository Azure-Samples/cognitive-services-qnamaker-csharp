# Cognitive Services QnA Maker Samples in C#

These samples show you how to programmatically create, update, publish, and replace a QnA Maker knowledge base, amongst many other ways to interact with it. All samples are in C#. To view these same samples in other languages:

[cognitive-services-qnamaker-java](https://github.com/Azure-Samples/cognitive-services-qnamaker-java)

[cognitive-services-qnamaker-nodejs](https://github.com/Azure-Samples/cognitive-services-qnamaker-nodejs)

[cognitive-services-qnamaker-python](https://github.com/Azure-Samples/cognitive-services-qnamaker-python)


## Features

Included are the following samples:

* [Create knowledge base](https://github.com/Azure-Samples/cognitive-services-qnamaker-python/blob/master/create-new-knowledge-base.cs). Create a brand new knowledge base with given FAQ URLs. You may supply your own.
* [Update knowledge base](https://github.com/Azure-Samples/cognitive-services-qnamaker-python/blob/master/update-knowledge-base.cs). Update an existing knowledge base by changing its name.
* [Publish knowledge base](https://github.com/Azure-Samples/cognitive-services-qnamaker-python/blob/master/publish-knowledge-base.cs). Publish any existing knowledge base to the host your Azure account.
* [Replace knowledge base](https://github.com/Azure-Samples/cognitive-services-qnamaker-python/blob/master/replace-knowledge-base.cs). Replace an entire existing knowledge base with a custom question/answer pair.
* [Download knowledge base](https://github.com/Azure-Samples/cognitive-services-qnamaker-python/blob/master/download-knowledge-base.cs). Download the contents of your existing knowledge base in JSON.

All samples revolve around what you can do with a knowledge base, which is made up of FAQs or product manuals where there is a question and an answer. QnA Maker gives you more control over how to answer questions by allowing you to train a chat bot to give answers in a variety of ways that feels more like natural, conversational exchanges.

<img src="https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/media/botFrameworkArch.png" width="700">

## Getting Started

### Prerequisites

For each sample, a subscription key is required from your Azure Portal account. 
* To create a new account/resource for QnA Maker, see [Create a Cognitive Services API account in the Azure portal](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account). You may need to 'Search in Marketplace' for QnA Maker if you don't see it in the list given.  
* For existing accounts, the key can be found in your [Azure Portal](https://ms.portal.azure.com/) dashboard in your QnA Maker resource under Resource Management > Keys. You'll need this key to add to your sample before running.

With the exception of creating a new knowledge base, these samples will require your [QnA Maker account](https://www.qnamaker.ai/Home/MyServices) knowledge base ID. 

### Installation

1. Create a C# Console App (.NET Framework) in Visual Studio 2017, then add the sample to Program.cs.

1. Install the Newtonsoft.Json NuGet package, then run the sample.

### Quickstart

* Quickstart: [Create a new knowledge base in C#](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/quickstarts/create-new-kb-csharp)
* Quickstart: [Update a knowledge base in C#](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/quickstarts/update-kb-csharp)
* Quickstart: [Publish a knowledge base in C#](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/quickstarts/publish-kb-csharp)
* More quickstarts coming soon... in the meantime, refer to [Quickstart for Microsoft QnA Maker API with C#](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/quickstarts/csharp) for all quickstarts in minimal format.
