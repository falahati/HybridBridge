./build.ps1
$msbuild = join-path -path (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0")."MSBuildToolsPath" -childpath "msbuild.exe"
&$msbuild HybridBridge\HybridBridge.csproj /t:Publish /p:Configuration="Release"
&$msbuild HybridBridge.Android\HybridBridge.Android.csproj /t:Publish /p:Configuration="Release"
&$msbuild HybridBridge.iOS\HybridBridge.iOS.csproj /t:Publish /p:Configuration="Release"
&$msbuild HybridBridge.UWP\HybridBridge.UWP.csproj /t:Publish /p:Configuration="Release"
&$msbuild HybridBridge.WebServer\HybridBridge.WebServer.csproj /t:Publish /p:Configuration="Release"
&$msbuild HybridBridge.Win\HybridBridge.Win.csproj /t:Publish /p:Configuration="Release"
&$msbuild HybridBridge.Win81\HybridBridge.Win81.csproj /t:Publish /p:Configuration="Release"