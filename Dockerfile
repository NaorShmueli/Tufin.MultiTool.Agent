# -------------------------
# Build stage
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \Tufin.MultiTool.Agent.API/Tufin.MultiTool.Agent.API.csproj

RUN dotnet publish \
    Tufin.MultiTool.Agent.API/Tufin.MultiTool.Agent.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore


# -------------------------
# Runtime stage
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

ENTRYPOINT ["dotnet", "Tufin.MultiTool.Agent.API.dll"]