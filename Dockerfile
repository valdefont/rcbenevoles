# Use the official ASP.NET image as a base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies for both projects
COPY ["web/web.csproj", "web/"]
COPY ["dal/dal.csproj", "dal/"]
RUN dotnet restore "web/web.csproj"

# Copy the entire solution and build the web project
COPY . .
WORKDIR "/src/web"
RUN dotnet build "web.csproj" -c Release -o /app/build

# Publish the web project
FROM build AS publish
RUN dotnet publish "web.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "web.dll"]
