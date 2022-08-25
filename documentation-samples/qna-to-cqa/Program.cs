namespace QnA2CQA
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;

    /// <summary>
    /// Tool to converts QnA Maker KB assets to CQA import assets.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">args.</param>
        public static void Main(string[] args)
        {
            dynamic importRequest = new ExpandoObject();


            /*
             * KB Details -> Project metadata
             */

            // Output of QnA Maker Knowledge base - Get details API
            // https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker4.0/knowledgebase/get-details?tabs=HTTP
            var kbDetailsLegacy = File.ReadAllText(@".\samples\qnaLegacy-KbDetails.json");
            if (kbDetailsLegacy != null)
            {
                var kbDetailsJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(kbDetailsLegacy);

                dynamic projectMetadata = new ExpandoObject();

                projectMetadata.projectName = kbDetailsJson.id;
                projectMetadata.description = kbDetailsJson.name;
                projectMetadata.language = kbDetailsJson.language;
                projectMetadata.defaultAnswer = "No answer found in knowledge base.";

                // KB details is equivalent to project metadata
                importRequest.metadata = projectMetadata;
            }

            /*
             * Alterations -> Synonyms
             * QnADocuments -> QnAs
             */

            dynamic assets = new ExpandoObject();


            // Output from QnA Maker Alterations Get API
            // https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker4.0/alterations/get?tabs=HTTP
            var synonymsLegacy = File.ReadAllText(@".\samples\qnaLegacy-Synonyms.json");

            // Synonyms in the project assets are equivalent of Word alterations in a QnA Maker service
            if (synonymsLegacy != null)
            {
                var synonymsJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(synonymsLegacy);
                assets.synonyms = synonymsJson.wordAlterations;
            }

            // Output of QnA Maker download API
            // https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker4.0/knowledgebase/download?tabs=HTTP
            // /{kbId}/test/qna
            var qnasLegacy = File.ReadAllText(@".\samples\qnaLegacy-QnAs.json");
            if (qnasLegacy != null)
            {

                var qnasJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(qnasLegacy);
                var qnas = new List<dynamic>();
                foreach (var qna in qnasJson.qnaDocuments)
                {
                    var qnaRecord = GetQnARecordFromQnALegacy(qna);
                    qnas.Add(qnaRecord);
                }

                // QnARecords of a project are equivalent to QnADocuments in a Knowledge base.
                assets.qnas = qnas;
            }

            importRequest.assets = assets;
            var importRequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(importRequest);

            File.WriteAllText(@".\samples\output.json", importRequestJson);
            // check this file in bin\Debug\...\samples or bin\Release\...\samples folder
            // This json output can be used as payload for custom question answering import API
            // https://docs.microsoft.com/en-us/rest/api/cognitiveservices/questionanswering/question-answering-projects/import?tabs=HTTP
            // Add URL parameter &format=Json
        }

        private static dynamic GetQnARecordFromQnALegacy(dynamic qna)
        {
            var qnaLegacy = qna == null ? throw new Exception() : qna;

            dynamic qnaRecord = new ExpandoObject();
            qnaRecord.id = qnaLegacy.id;
            qnaRecord.answer = qnaLegacy.answer;
            qnaRecord.source = qnaLegacy.source;
            qnaRecord.sourceDisplayName = string.Empty;

            var questions = new List<dynamic>();
            for (var q = 0; q < qnaLegacy.questions.Count; q++)
            {
                questions.Add(qnaLegacy.questions[q]);
            }

            qnaRecord.questions = questions;
            dynamic metadata = new ExpandoObject();
            var numMetadata = qnaLegacy?.metadata?.Count ?? 0;
            for (var m = 0; m < numMetadata; m++)
            {
                AddProperty(metadata, (string)qnaLegacy.metadata[m].name, qnaLegacy.metadata[m].value as object);
            }

            qnaRecord.metadata = metadata;
            dynamic dialog = new ExpandoObject();
            var numPrompts = qnaLegacy?.context?.prompts?.Count ?? 0;
            var promptsArray = new List<dynamic>();
            dialog.isContextOnly = qnaLegacy.context.isContextOnly;

            for (var p = 0; p < numPrompts; p++)
            {
                dynamic prompt = new ExpandoObject();
                prompt.qnaId = qnaLegacy.context.prompts[p].qnaId;
                prompt.displayText = qnaLegacy.context.prompts[p].displayText;
                prompt.displayOrder = qnaLegacy.context.prompts[p].displayOrder;
                promptsArray.Add(prompt);
            }

            dialog.prompts = promptsArray;
            qnaRecord.dialog = dialog;

            var hasAlternateQuestions = qnaLegacy?.alternateQuestionClusters?.Count > 0;
            qnaRecord.activeLearningSuggestions = new List<dynamic>();

            if (hasAlternateQuestions)
            {
                var numAlternateQuestionClusters = qnaLegacy.alternateQuestionClusters.Count;
                for (var a = 0; a < numAlternateQuestionClusters; a++)
                {
                    dynamic cluster = new ExpandoObject();
                    cluster.clusterHead = qnaLegacy.alternateQuestionClusters[a].clusterHead;
                    cluster.totalAutoSuggestedCount = qnaLegacy.alternateQuestionClusters[a].totalAutoSuggestedCount;
                    cluster.totalUserSuggestedCount = qnaLegacy.alternateQuestionClusters[a].totalUserSuggestedCount;

                    var alternateQuestionList = new List<dynamic>();
                    var numAlternateQuestions = qnaLegacy.alternateQuestionClusters[a].alternateQuestionList.Count;
                    for (var q = 0; q < numAlternateQuestions; q++)
                    {
                        dynamic alternateQuestion = new ExpandoObject();
                        alternateQuestion.question = qnaLegacy.alternateQuestionClusters[a].alternateQuestionList[q].question;
                        alternateQuestion.autoSuggestedCount = qnaLegacy.alternateQuestionClusters[a].alternateQuestionList[q].autoSuggestedCount;
                        alternateQuestion.userSuggestedCount = qnaLegacy.alternateQuestionClusters[a].alternateQuestionList[q].userSuggestedCount;
                        alternateQuestionList.Add(alternateQuestion);
                    }
                    cluster.suggestedQuestions = alternateQuestionList;
                    qnaRecord.activeLearningSuggestions.Add(cluster);
                }
            }

            qnaRecord.isDocumentText = qnaLegacy.isDocumentText;
            return qnaRecord;
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var exDict = expando as IDictionary<string, object>;
            if (exDict.ContainsKey(propertyName))
            {
                exDict[propertyName] = propertyValue;
            }
            else
            {
                exDict.Add(propertyName, propertyValue);
            }
        }
    }
}
