# Migrate QnA Maker JSON assets to Custom question answering project

## Pre requisites
a. Developer background
b. Output of the below APIs in JSON format
1. KB Details - https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker4.0/knowledgebase/get-details?tabs=HTTP 
Save it as qnaLegacy-KbDetails.json in samples folder
2. Alterations - https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker4.0/alterations/get?tabs=HTTP
Save it as qnaLegacy-Synonyms.json in samples folder
3. QnAs - https://docs.microsoft.com/en-us/rest/api/cognitiveservices-qnamaker/qnamaker4.0/knowledgebase/download?tabs=HTTP
Save it as qnaLegacy-QnAs.json in samples folder

## Run the program
`QnA2CQA.exe` 

Follow code and code comments in Program.cs for exact mapping between
- KB Details -> Project Metadata
- Alterations -> Synonyms
- QnADocuments -> QnA Records