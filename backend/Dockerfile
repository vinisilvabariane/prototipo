FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MeOne.API/MeOne.API.csproj", "MeOne.API/"]
COPY ["MeOne.Application/MeOne.Application.csproj", "MeOne.Application/"]
COPY ["MeOne.Domain/MeOne.Domain.csproj", "MeOne.Domain/"]
COPY ["MeOne.Infrastructure/MeOne.Infrastructure.csproj", "MeOne.Infrastructure/"]
RUN dotnet restore "MeOne.API/MeOne.API.csproj"

COPY . .
RUN dotnet publish "MeOne.API/MeOne.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MeOne.API.dll"]
