# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish ./
RUN apt-get update && apt-get install -y libgpiod2 && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "FanCommander.dll"]
