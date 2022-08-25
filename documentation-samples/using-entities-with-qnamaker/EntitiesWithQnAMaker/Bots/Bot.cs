// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio Bot v4.13.2

namespace EntitiesWithQnAMaker.Bots
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using EntitiesWithQnAMaker.LuisHelper;

    /// <summary>
    /// Bot Class.
    /// </summary>
    public class Bot : ActivityHandler
    {
        private readonly LuisHelper luisHelper;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        /// <param name="luisHelper">luisHelper.</param>
        /// <param name="endpoint">qna maker endpoint.</param>
        /// <param name="configuration">configuration.</param>
        public Bot(LuisHelper luisHelper, QnAMakerEndpoint endpoint, IConfiguration configuration)
        {
            this.luisHelper = luisHelper;
            this.configuration = configuration;
            this.QnaMaker = new QnAMaker(endpoint);
        }

        /// <summary>
        /// Gets property.
        /// </summary>
        public QnAMaker QnaMaker { get; private set; }

        /// <inheritdoc/>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome! \n\n I can help you with queries about surface pro models like charging time of surface pro 4, battery life of surface pro, etc. ";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var luisResult = await this.luisHelper.RecognizeAsync<LaptopManual>(turnContext, cancellationToken);
            var topIntent = luisResult.TopIntent().intent;

            switch (topIntent)
            {
                case LaptopManual.Intent.MicrosoftSurfaceModel:
                    // model variable contains all the entities extracted from query.
                    var model = luisResult?.Entities?.Model;

                    // if no model is found, then we show result for surface pro 4
                    var metaDataArray = new Metadata[1] { new Metadata() { Name = "MetadataName", Value = "surface pro 4" } };

                    if (model is not null)
                    {
                        // creating list of metadatas for all the models found.
                        metaDataArray = model
                       .Select(s => new Metadata() { Name = "MetadataName", Value = $"{s[0]}" })
                       .ToArray();
                    }

                    // any custom filter can be added here.
                    var qnaOptions = new QnAMakerOptions
                    {
                        ScoreThreshold = float.Parse(this.configuration["QnAScoreThreshold"]),     // Minimum threshold score for answers.
                        Top = int.Parse(this.configuration["QnATop"]),    // Max number of answers to be returned for the question.
                        StrictFilters = metaDataArray,       // Find QnAs that are associated with the given list of metadata.
                        StrictFiltersJoinOperator = JoinOperator.OR,     // using OR operations to get results for all models.
                    };

                    // The actual call to the QnA Maker service.
                    var response = await this.QnaMaker.GetAnswersAsync(turnContext, qnaOptions);
                    if (response.Length > 0)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("No good match found in kb"), cancellationToken);
                    }

                    break;

                default:
                    // default reply, take care of none intent
                    var replyText = $"Hi there, I am unable to help you with your question. Please try again by rephrasing your query.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                    break;
            }
        }
    }
}
