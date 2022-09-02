$asterisk = "\*"
$PublishPath = "C:\Users\t-abelseyoum\source\repos\microsoft\cre-avengers-interns\ALTA\src\NetCore\6.0\Microsoft.ALTA\Microsoft.ALTA\bin\Release\net6.0\win-x64\publish"
$TestBinariesPath = "C:\Users\t-abelseyoum\source\repos\microsoft\cre-avengers-interns\ALTA\src\NetCore\STLTest\bin\Release\net6.0\win-x64\*"
$ZipPath = "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\net6.0.zip"
$FullPublishPath = $PublishPath+$asterisk
$Subscription = "Cloud.Validation.and.Pipeline.Orchestration"
$ResourceGroup = "rg-test-internship"
$WebApp = "STLZipDeployChecker"


$fileexists = Test-Path -Path $PublishPath
$zipexists = Test-Path -Path $ZipPath

If($zipexists){
    Remove-Item $ZipPath -Recurse
}

If($fileexists){
   Remove-Item $FullPublishPath -Recurse
}

Copy-Item $TestBinariesPath -Destination $PublishPath -Force
Compress-Archive -Path $FullPublishPath "net6.0.zip"
az account set --name $Subscription
az webapp deploy --resource-group $ResourceGroup --name $WebApp --type "zip" --src-path $ZipPath

