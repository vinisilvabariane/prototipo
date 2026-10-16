FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MeOne.API/MeOne.API.csproj", "MeOne.API/"]
COPY ["MeOne.Application/MeOne.Application.csproj", "MeOne.Application/"]
COPY ["MeOne.Domain/MeOne.Domain.csproj", "MeOne.Domain/"]
COPY ["MeOne.Infrastructure/MeOne.Infrastructure.csproj", "MeOne.Infrastructure/"]
RUN dotnet restore "MeOne.API/MeOne.API.csproj"

COPY . .
RUN dotnet publish "MeOne.API/MeOne.API.csproj" -c Release -o /app/publish
RUN mkdir -p /data/workflow-uploads /data/work-instruction-uploads /data/supplier-uploads

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build --chown=1654:1654 /data /data

ENV ASPNETCORE_HTTP_PORTS=8080
ENV Workflow__Storage__Path=/data/workflow-uploads
ENV WorkInstruction__Storage__Path=/data/work-instruction-uploads
ENV Supplier__Storage__Path=/data/supplier-uploads
VOLUME /data/workflow-uploads
VOLUME /data/work-instruction-uploads
VOLUME /data/supplier-uploads
EXPOSE 8080

ENTRYPOINT ["dotnet", "MeOne.API.dll"]
