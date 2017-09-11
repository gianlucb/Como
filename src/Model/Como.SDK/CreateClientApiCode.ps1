# This script creates the client for the FrontEnd API automatically
# it uses AUTOREST (https://github.com/Azure/AutoRest)
#   - starting from json file from swagger creates C# files
#  to run first you need to install AUTOREST with npm install -g autorest
#  then deploy the FrontEndType service and execute this script

$autorestExe= $env:APPDATA + "\npm\autorest.cmd";
$swaggerFrontEndFile = "swagger_frontend_api.json";

#clean old definition files
If (Test-Path $swaggerFrontEndFile){
	Remove-Item $swaggerFrontEndFile
}

Remove-Item *.log
Remove-Item *.cs

Invoke-WebRequest -Uri http://localhost:80/swagger/v1/swagger.json -OutFile $swaggerFrontEndFile;


Start-Process -FilePath $autorestExe -NoNewWindow -ArgumentList "--input-file=$swaggerFrontEndFile --csharp --output-folder=. --namespace=Como.SDK" -RedirectStandardOutput log_frontend.log -RedirectStandardError log_frontend_error.log


