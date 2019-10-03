
Usage Instructions
I Build Instructions:
1. Open the sln file in Visual Studio 2017
2. Hit F6

Run command as below–
dotnet batchtesting.dll FAQ_Sample.tsv https://activelearningtest.azurewebsites.net edcf3783-8303-4c44-8014-03c55b5f8f0a OUTPUT_new.tsv


II Non VS Build(Uses Full .net FX, no need for core runtime): https://qnamakerstore.blob.core.windows.net/qnamakerdata/batchtesting/bt.zip
Run command as below–
batchtesting.exe FAQ_Sample.tsv https://activelearningtest.azurewebsites.net edcf3783-8303-4c44-8014-03c55b5f8f0a OUTPUT_new.tsv


Parameters -

· Input file name in tsv format – Need to give the input file path

You need to create the sample input data for batch testing in a format (below)

6251001c-273b-4a60-8325-4ccbac07f568(KBID) Question name:value* N(Top)*

*Metadata and Top parameters are optional

· Hostname – Hostname details

· Endpoint key – Endpoint key details

· Output file name in tsv format – Need to give the output file path
