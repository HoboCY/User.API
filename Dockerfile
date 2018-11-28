FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY User.API/*.csproj ./User.API/
RUN dotnet restore

# copy everything else and build app
COPY User.API/. ./User.API/
WORKDIR /app/User.API
RUN dotnet publish /property:PublishWithAspNetCoreTargetManifest=false -c Release -o out


FROM microsoft/dotnet:2.0-runtime AS runtime
WORKDIR /app
COPY --from=build /app/User.API/out ./
ENTRYPOINT ["dotnet", "User.API.dll"]
