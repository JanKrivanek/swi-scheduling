FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ./SchedulerSample.sln .
COPY ./Directory.Packages.props .
COPY ./src/TenantHelper/TenantHelper.csproj src/TenantHelper/TenantHelper.csproj
COPY ./src/WorkInserter/WorkInserter.csproj src/WorkInserter/WorkInserter.csproj
COPY ./NuGet.config .


#RUN dotnet restore "InMemoryCaching.sln"
RUN dotnet restore "src/WorkInserter/WorkInserter.csproj"

COPY . .
WORKDIR "/src/."

RUN dotnet publish "src/WorkInserter/WorkInserter.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkInserter.dll"]