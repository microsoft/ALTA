#delete everything in ALTA bin folder
#copy items from test bin into alta bin folder
#then build alta
#then build docker image

Remove-Item "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA\Microsoft.ALTA\bin\Debug\net6.0\*" -Recurse
Copy-Item "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA.SampleTest\Microsoft.ALTA.SampleTest\bin\Debug\net6.0\*" -Destination "C:\Users\t-abelseyoum\source\repos\microsoft\ALTA\src\NetCore\Microsoft.ALTA\Microsoft.ALTA\bin\Debug\net6.0"