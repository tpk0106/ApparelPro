# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project files for restore
COPY ApparelPro.WebApi/ApparelPro.WebApi.csproj ApparelPro.WebApi/
COPY ApparelPro.Data/ApparelPro.Data.csproj ApparelPro.Data/
COPY apparelPro.BusinessLogic/apparelPro.BusinessLogic.csproj apparelPro.BusinessLogic/
COPY ApparelPro.Shared/ApparelPro.Shared.csproj ApparelPro.Shared/

# Restore dependencies
RUN dotnet restore ApparelPro.WebApi/ApparelPro.WebApi.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish ApparelPro.WebApi/ApparelPro.WebApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApparelPro.WebApi.dll"]