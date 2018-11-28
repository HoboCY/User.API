FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

# copy csproj and restore as distinct layers
FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY * ./
COPY User.API.csproj User.API/
RUN dotnet restore
COPY . .
WORKDIR /src/User.API
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet","User.API.dll"]
