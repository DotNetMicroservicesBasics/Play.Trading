# Play.Trading
Play Economy Trading microservice

## Build the docker image
```powershell
$version="1.0.1"
$env:GH_OWNER="DotNetMicroservicesBasics"
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.trading:$version .
```

## Run the docker image
```powershell
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMqSettings__Host=rabbitmq --network playinfrastructure_default play.trading:$version
```