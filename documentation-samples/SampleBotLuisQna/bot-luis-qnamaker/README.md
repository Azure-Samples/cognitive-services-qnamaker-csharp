# bot_luis_qnamaker

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a bot that uses Luis to extract entities and qna maker to query the knowledge base using the extracted entities as metadata in strict filters.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## To try this sample

  ### Create a QnA Maker service and publish a knowledge base 
  - Create a [qna maker resource](https://ms.portal.azure.com/#create/Microsoft.CognitiveServicesQnAMaker) 
  - Sign in to [the QnAMaker.ai](https://qnamaker.ai/) portal with your Azure credentials.
  - In the QnA Maker portal, create a knowledge base using the LaptopManualKb.tsv file located in the 'Models' folder of the sample project. Name your knowledge base qna, and use the LaptopManualKb.tsv file to populate it.
  
  ### Obtain values to connect your bot to the knowledge base
  - In the [QnA Maker](https://www.qnamaker.ai/) site, select your knowledge base.
  - With your knowledge base open, select the SETTINGS tab. 
  - Scroll down to find Deployment details and record the following values from the Postman sample HTTP request:
    - POST /knowledgebases/<knowledge-base-id>/generateAnswer
    - Host: <YOUR_HOST_URL>
    - Authorization: EndpointKey <YOUR_ENDPOINT_KEY>
  - Your host URL will start with https:// and end with /qnamaker, such as https://.azure.net/qnamaker. Your bot will need the knowledge base ID, host URL, and endpoint key to connect to your QnA Maker knowledge base.

  ### Create a LUIS app in the LUIS portal
  - Sign in to the [LUIS](https://www.luis.ai/) portal.
     - If you don't already have an account, create one.
    - If you don't already have an authoring resource, create one.
    -  For more information, see the LUIS documentation on [how to sign in to the LUIS portal](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-start-new-app#sign-in-to-luis-portal).
  - On the My Apps page, click New app for conversation and select Import as JSON.
  - Import the LaptopManual.json file located in the 'Models' folder of the sample project.
  - Train and publish your app. For more information, see the LUIS documentation on how to [train](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/luis-how-to-train) and [publish](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/publishapp) an app to the production environment.

  ### Retrieve application information from the LUIS.ai portal
  - Select your published LUIS app from [luis.ai](https://www.luis.ai/).
  - With your published LUIS app open, select the MANAGE tab.
  - Select the Settings tab on the left side and record the value shown for Application ID as <YOUR_APP_ID>.
  - Select the Azure Resources tab on the left side and select the Authoring Resource group. Record the value shown for Location as <YOUR_REGION> and Primary Key as <YOUR_AUTHORING_KEY>.

  ### Update the settings file

  - If you aren't deploying the bot for production, your bot's app ID and password fields can be left blank.

  - Add the information required to access your LUIS app including application id, authoring key, and region into the appsettings.json file. These are the values you saved previously from your published LUIS app. Note that the API host name should be in the format <your_region>.api.cognitive.microsoft.com.

  - Add the information required to access your knowledge base including hostname, endpoint key and knowledge base ID (kbId) into the settings file. These are the values you saved from the SETTINGS tab of your knowledge base in QnA Maker.
  - Add the Top & Score threshold for answers to be returned for the question from Kb.

    ```javascript
        {
        "MicrosoftAppId": "",
        "MicrosoftAppPassword": "",

        "LuisAppId": "",
        "LuisAPIKey": "",
        "LuisAPIHostName": "",

        "QnAAuthKey": "",
        "QnAEndpointHostName": "",
        "QnAKnowledgebaseId": "",
        "QnATop": <Top>,
        "QnAScoreThreshold": <Score_Threshold>
        }
    ```

- Next step in a terminal, navigate to `bot_luis_qnamaker`

    ```bash
    # change into project folder
    cd # bot_luis_qnamaker
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `bot_luis_qnamaker` folder
  - Select `SampleBotLuisQna.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.5.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
