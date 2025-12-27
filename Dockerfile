# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FiapCloudPayment/PaymentService/PaymentService.csproj PaymentService/
COPY SharedMessages/SharedMessages.csproj SharedMessages/

RUN dotnet restore PaymentService/PaymentService.csproj

COPY FiapCloudPayment/PaymentService/. PaymentService/
COPY SharedMessages/. SharedMessages/

RUN dotnet publish PaymentService/PaymentService.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "PaymentService.dll"]
