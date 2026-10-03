FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
# default port (the platform's PORT variable overrides it)
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CampusEats.Api.dll"]
