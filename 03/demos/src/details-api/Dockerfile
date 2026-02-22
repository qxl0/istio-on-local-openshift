FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src
COPY BookInfo.Details.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out BookInfo.Details.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:6.0

ENV ASPNETCORE_URLS=http://+:9080
EXPOSE 9080

WORKDIR /app
ENTRYPOINT ["dotnet", "BookInfo.Details.dll"]

COPY --from=builder /out/ .