$solution = 'src\Como.sln'
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe'
nuget restore $solution

& $msbuild $solution /p:platform="x64" /p:configuration="Debug"

Get-ChildItem -Path 'src' -Filter "*.sfproj" -Recurse | ForEach-Object {
    $fullName = $_.FullName

    Write-Host "Building Binaries $fullName"

    Write-Host "Packaging Service Fabric Apps"
   
    & $msbuild  "$fullName" /t:Package /p:platform="x64" /p:configuration="Debug"

}