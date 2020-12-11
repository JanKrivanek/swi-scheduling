FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ./SchedulerSample.sln .
COPY ./Directory.Packages.props .
COPY ./src/TenantHelper/TenantHelper.csproj src/TenantHelper/TenantHelper.csproj
COPY ./src/WorkExecutor/WorkExecutor.csproj src/WorkExecutor/WorkExecutor.csproj
COPY ./NuGet.config .

#RUN dotnet restore "InMemoryCaching.sln"
RUN dotnet restore "src/WorkExecutor/WorkExecutor.csproj"

COPY . .
WORKDIR "/src/."

RUN dotnet publish "src/WorkExecutor/WorkExecutor.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkExecutor.dll"]