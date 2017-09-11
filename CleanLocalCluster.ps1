Connect-ServiceFabricCluster

$apps = Get-ServiceFabricApplication
Foreach ($a in $apps)
{
    Remove-ServiceFabricApplication -ApplicationName $a.ApplicationName  -Confirm
}

Unregister-ServiceFabricApplicationType -ApplicationTypeName Como.ClusterType -ApplicationTypeVersion 1.0.0
Unregister-ServiceFabricApplicationType -ApplicationTypeName Como.Public.EventType -ApplicationTypeVersion 1.0.0

