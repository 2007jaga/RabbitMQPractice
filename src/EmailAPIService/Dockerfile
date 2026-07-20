# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy the entire solution
COPY . .

# Move to the EmailAPI project
WORKDIR /src/src/EmailAPIService

# Restore dependencies
RUN dotnet restore "EmailAPIService.csproj"

# Publish the application
RUN dotnet publish "EmailAPIService.csproj" -c Release -o /app/publish --no-restore

# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EmailAPIService.dll"]

# (base) PS C:\F-Drive\RabbitMQPractice> docker build --no-cache --progress=plain -t emailapi:1.0 -f src/EmailAPIService/Dockerfile .
# (base) PS C:\F-Drive\RabbitMQPractice> docker run -d --name emailapi -p 8081:8080 emailapi:1.0