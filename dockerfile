# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy .csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Serve the app
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port (Render uses PORT env var)
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 80

ENTRYPOINT ["dotnet", "SocietyApp.dll"]
