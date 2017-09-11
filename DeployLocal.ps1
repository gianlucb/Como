Connect-ServiceFabricCluster
Import-Module "$ENV:ProgramFiles\Microsoft SDKs\Service Fabric\Tools\PSModule\ServiceFabricSDK\ServiceFabricSDK.psm1"
$publicEventPackage = 'src\Public\Como.Public.Event\pkg\Debug'
$clusterPackage = 'src\Cluster\Como.Cluster\pkg\Debug'

Test-ServiceFabricApplicationPackage $publicEventPackage -ErrorAction Stop
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $publicEventPackage -ApplicationPackagePathInImageStore Como.Public.Event -ImageStoreConnectionString (Get-ImageStoreConnectionStringFromClusterManifest(Get-ServiceFabricClusterManifest)) -TimeoutSec 1800
Register-ServiceFabricApplicationType Como.Public.Event

Test-ServiceFabricApplicationPackage $clusterPackage -ErrorAction Stop
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $clusterPackage -ApplicationPackagePathInImageStore Como.Cluster -ImageStoreConnectionString (Get-ImageStoreConnectionStringFromClusterManifest(Get-ServiceFabricClusterManifest)) -TimeoutSec 1800
Register-ServiceFabricApplicationType Como.Cluster
New-ServiceFabricApplication -ApplicationName fabric:/Como.Cluster -ApplicationTypeName Como.ClusterType -ApplicationTypeVersion '1.0.0' -ErrorAction Stop
Write-Host "Done."



