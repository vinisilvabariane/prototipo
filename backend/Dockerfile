FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["IrisWeb.API/IrisWeb.API.csproj", "IrisWeb.API/"]
COPY ["IrisWeb.Application/IrisWeb.Application.csproj", "IrisWeb.Application/"]
COPY ["IrisWeb.Domain/IrisWeb.Domain.csproj", "IrisWeb.Domain/"]
COPY ["IrisWeb.Infrastructure/IrisWeb.Infrastructure.csproj", "IrisWeb.Infrastructure/"]
RUN dotnet restore "IrisWeb.API/IrisWeb.API.csproj"

COPY . .
RUN dotnet publish "IrisWeb.API/IrisWeb.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IrisWeb.API.dll"]
