# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG GH_USERNAME
ARG GH_TOKEN

RUN dotnet nuget add source https://nuget.pkg.github.com/${GH_USERNAME}/index.json \
    --name github \
    --username ${GH_USERNAME} \
    --password ${GH_TOKEN} \
    --store-password-in-clear-text

COPY PaymentService/PaymentService.csproj PaymentService/
RUN dotnet restore PaymentService/PaymentService.csproj

COPY PaymentService/. PaymentService/
RUN dotnet publish PaymentService/PaymentService.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "PaymentService.dll"]
