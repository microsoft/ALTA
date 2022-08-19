#delete everything in ALTA bin folder
#copy items from test bin into alta bin folder
#then build alta
#then zip everything in ALTA
#then zip deploy to azure
#then build docker image


Remove-Item "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA\Microsoft.ALTA\bin\Debug\net6.0\*" -Recurse
Copy-Item "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA.SampleTest\Microsoft.ALTA.SampleTest\bin\Debug\net6.0\*" -Destination "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA\Microsoft.ALTA\bin\Debug\net6.0"
dotnet build "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA\Mircosoft.ALTA.sln"
Compress-Archive -Path "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA\Microsoft.ALTA\bin\Debug\net6.0" "net6.0.zip"
az account set --name "Cloud.Validation.and.Pipeline.Orchestration"
#az webapp config appsettings set --resource-group "rg-test-internship" --name "ZipDeployChecker" --settings WEBSITE_RUN_FROM_PACKAGE="1"
#az webapp deployment source config-zip --resource-group "rg-test-internship" --name "ZipDeployChecker" --src "net6.0.zip"
az webapp deploy --resource-group "rg-test-internship" --name "ZipDeployChecker" --type "zip" --src-path "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\net6.0.zip"
#Publish-AzWebApp -ResourceGroupName Default-Web-WestUS -Name ZipDeployChecker2 -ArchivePath "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\net6.0.zip"

