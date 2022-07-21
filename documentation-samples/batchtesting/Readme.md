# Batch testing your knowledge base

Batch testing is available from this source code. The format of the command to run the batch test is:

```console
For Qna Maker GA: batchtesting.exe input.tsv https://YOUR-HOST.azurewebsites.net ENDPOINT-KEY out.tsv
```

```console
For QnA Maker managed (preview): batchtesting.exe input.tsv https://YOUR-HOST.cognitiveservices.azure.com COGNITIVE-SERVICE-KEY out.tsv
```

|Param|Expected Value|
|--|--|
|1|name of tsv file formatted with [TSV input fields](#tsv-input-fields)|
|2|URI for endpoint, with YOUR-HOST from the Publish page of the QnA Maker portal.|
|3|ENDPOINT-KEY, found on Publish page of the QnA Maker portal.|
|4|name of tsv file created by batch test for results.|

Use the following information to understand and implement the TSV format for batch testing. 

## TSV input fields

|TSV input file fields|Notes|
|--|--|
|KBID|Your KB ID found on the Publish page. or ProjectName when using for batch testing Language Custom question answering project|
|Question|The question a user would enter.|
|Metadata tags|optional|
|Top parameter|optional| 
|Expected answer ID|optional|

![Input format for TSV file for batch testing.](../media/input-tsv-format-batch-test.png)

## TSV output fields for Qna Maker GA

|TSV Output file parameters|Notes|
|--|--|
|KBID|Your KB ID found on the Publish page.|
|Question|The question as entered from the input file.|
|Answer|Top answer from your knowledge base.|
|Answer ID|Answer ID|
|Score|Prediction score for answer. |
|Metadata tags|associated with returned answer|
|Expected answer ID|optional (only when expected answer ID is given in input file)|
|Judgment label|optional, values could be: correct or incorrect (only when expected answer id is given in input file)|

## TSV output fields for QnA Maker managed (preview)

|TSV Output file parameters|Notes|
|--|--|
|KBID|Your KB ID found on the Publish page.|
|Question|The question as entered from the input file.|
|Answer|Top answer from your knowledge base.|
|Answer ID|Answer ID|
|Score|Prediction score for answer. |
|AnswerSpanText|Precise short answer for the query, if present. |
|AnswerSpanScore|Prediction score for short answer, if present. |
|Metadata tags|associated with returned answer|
|Expected answer ID|optional (only when expected answer ID is given in input file)|
|Judgment label|optional, values could be: correct or incorrect (only when expected answer id is given in input file)|

## TSV output fields for Language custom question answering project

|TSV Output file parameters|Notes|
|--|--|
|ProjectName| Language - Custom question answering project name|
|Question|The question as entered from the input file.|
|Answer|Top answer from your knowledge base.|
|Answer ID|Answer ID|
|Score|Prediction score for answer. |
|AnswerSpanText|Precise short answer for the query, if present. |
|AnswerSpanScore|Prediction score for short answer, if present. |
|Metadata tags|associated with returned answer|
|Expected answer ID|optional (only when expected answer ID is given in input file)|
|Judgment label|optional, values could be: correct or incorrect (only when expected answer id is given in input file)|

## References
[Language QuestionAnswering SDK Samples](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/cognitivelanguage/Azure.AI.Language.QuestionAnswering/samples)