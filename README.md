# Play.Trading
Play Economy Trading microservice

## Build the docker image
```powershell
$version="1.0.3"
$env:GH_OWNER="DotNetMicroservicesBasics"
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.trading:$version .
```

## Run the docker image on local machine
```powershell
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMqSettings__Host=rabbitmq --network playinfrastructure_default play.trading:$version
```


## Run the docker image on Azure
```powershell
$cosmosDbConnectionString="[CONNECTION_STRING HERE]"
$serviceBusConnetionString="[CONNECTION_STRING HERE]"
$messageBroker="AZURESERVICEBUS"
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__ConnectionString=$cosmosDbConnectionString -e ServiceSettings__MessageBroker=$messageBroker -e ServiceBusSettings__ConnectionString=$serviceBusConnetionString play.trading:$version
```


## Publish the docker image on Azure
```powershell
$acrname="playeconomyazurecontainerregistry"
docker tag play.trading:$version "$acrname.azurecr.io/play.trading:$version"
az acr login --name $acrname
docker push "$acrname.azurecr.io/play.trading:$version"
```

## Create kubernetes namespace
```powershell
$namespace="playtrading"
kubectl create namespace $namespace
```

## Create kubernetes pod
```powershell
kubectl apply -f .\kubernetes\trading.yaml -n $namespace

# list pods in namespace
kubectl get pods -n $namespace -w

# output pod logs
$podname="playtrading-deployement-fbdd45d8b-pdsvn"
kubectl logs $podname -n $namespace

# list pod details
kubectl describe pod $podname -n $namespace

# list services (see puplic ip)
kubectl get services -n $namespace

# see events
kubectl get events -n $namespace

# list deployments
kubectl get deployments -n $namespace

# delete deployment
kubectl delete deployment $namespace-deployement -n $namespace

# delete service
kubectl delete service $namespace-service -n $namespace

# delete service account
kubectl delete serviceaccount $namespace-serviceaccount -n $namespace
```

## Create Azure Managed Identity and granting it access to Key Vault secrets
```powershell
$appname="playeconomy"
az identity create --resource-group $appname --name $namespace

$kvname="playeconomyazurekeyvault"
$IDENTITY_CLIENT_ID=az identity show -g $appname -n $namespace --query clientId -otsv
az keyvault set-policy -n $kvname --secret-permissions get list --spn $IDENTITY_CLIENT_ID
```

## Establish the federated identity credential
```powershell
$aksname="playeconomyakscluster"
$AKS_OIDC_ISSUER=az aks show -n $aksname -g $appname --query "oidcIssuerProfile.issuerUrl" -otsv

az identity federated-credential create --name $namespace --identity-name $namespace --resource-group $appname --issuer $AKS_OIDC_ISSUER --subject system:serviceaccount:"${namespace}":"${namespace}-serviceaccount"
```

## Install Helm Chart
```powershell
helm install playcatalog-svc .\helm -f .\helm\values.yaml -n $namespace
```

## Install Helm Chart from Container Registery
```powershell

$helmUser=[guid]::Empty.Guid
$helmPassword=az acr login --name $acrname --expose-token --query accessToken -o tsv

helm registry login "$acrname.azurecr.io" --username $helmUser --password $helmPassword

$hemlChartVersion="0.1.0"

helm upgrade --install playtrading-svc oci://$acrname.azurecr.io/helm/microservice --version $hemlChartVersion -f .\helm\values.yaml -n $namespace

# if failed add --debug to see more info
helm upgrade --install playtrading-svc oci://$acrname.azurecr.io/helm/microservice --version $hemlChartVersion -f .\helm\values.yaml -n $namespace --debug

# to make sure helm Charts cash updated
helm repo update
```

## Required repository secrets for Github workflow
```powershell
NUGET_READ_PAT: Created in GitHub user profile --> Settings --> Developer settings --> Personal access token
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
```