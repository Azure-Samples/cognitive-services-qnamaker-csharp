namespace EntitiesWithQnAMaker.LuisHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.Luis;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// luis helper class.
    /// </summary>
    public class LuisHelper : IRecognizer
    {
        private readonly LuisRecognizer recognizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisHelper"/> class.
        /// </summary>
        /// <param name="configuration">configuration.</param>
        public LuisHelper(IConfiguration configuration)
        {
            var luisIsConfigured = !string.IsNullOrEmpty(configuration["LuisAppId"]) && !string.IsNullOrEmpty(configuration["LuisAPIKey"]) && !string.IsNullOrEmpty(configuration["LuisAPIHostName"]);
            if (luisIsConfigured)
            {
                var luisApplication = new LuisApplication(
                    configuration["LuisAppId"],
                    configuration["LuisAPIKey"],
                    "https://" + configuration["LuisAPIHostName"]);

                var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
                {
                    PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                    {
                        IncludeInstanceData = true,
                    },
                };

                this.recognizer = new LuisRecognizer(recognizerOptions);
            }
        }

        /// <summary>
        /// Gets a value indicating whether returns true if luis is configured in the appsettings.json and initialized.
        /// </summary>
        public virtual bool IsConfigured => this.recognizer != null;

        /// <summary>
        /// Method.
        /// </summary>
        /// <param name="turnContext">turnContext.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>.</returns>
        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => await this.recognizer.RecognizeAsync(turnContext, cancellationToken);

        /// <summary>
        /// Method.
        /// </summary>
        /// <typeparam name="T">T.</typeparam>
        /// <param name="turnContext">turnContext.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>.</returns>
        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
            => await this.recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
    }
}
