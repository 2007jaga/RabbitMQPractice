# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy the entire solution
COPY . .

# Move to the OrderAPI project
WORKDIR /src/src/OrderAPIService

# Restore dependencies
RUN dotnet restore "OrderAPIService.csproj"

# Publish the application
RUN dotnet publish "OrderAPIService.csproj" -c Release -o /app/publish --no-restore

# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderAPIService.dll"]


# Build the docker file in side the path
# PS C:\F-Drive\RabbitMQPractice> docker build -t orderapi:1.0 -f src/OrderAPIService/Dockerfile .

# Run
# PS C:\F-Drive\RabbitMQPractice> docker run -d --name orderapi -p 8080:8080   orderapi:1.0     