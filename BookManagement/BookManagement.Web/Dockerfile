FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BookManagement.slnx ./
COPY BookManagement.Web/BookManagement.Web.csproj BookManagement.Web/
COPY BookManagement.Core/BookManagement.Core.csproj BookManagement.Core/
COPY BookManagement.Infrastructure/BookManagement.Infrastructure.csproj BookManagement.Infrastructure/

# Restore dependencies
RUN dotnet restore BookManagement.Web/BookManagement.Web.csproj

# Copy the rest of the code
COPY . .

# Build and publish
WORKDIR /src/BookManagement.Web
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BookManagement.Web.dll"]
