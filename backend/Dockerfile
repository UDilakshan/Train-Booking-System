# syntax=docker/dockerfile:1

# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY RailwayReservation.sln .
COPY src/RailwayReservation.Domain/*.csproj src/RailwayReservation.Domain/
COPY src/RailwayReservation.Application/*.csproj src/RailwayReservation.Application/
COPY src/RailwayReservation.Infrastructure/*.csproj src/RailwayReservation.Infrastructure/
COPY src/RailwayReservation.Api/*.csproj src/RailwayReservation.Api/
COPY tests/RailwayReservation.UnitTests/*.csproj tests/RailwayReservation.UnitTests/
COPY tests/RailwayReservation.IntegrationTests/*.csproj tests/RailwayReservation.IntegrationTests/

RUN dotnet restore RailwayReservation.sln

COPY . .
RUN dotnet publish src/RailwayReservation.Api/RailwayReservation.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN adduser -D -u 1000 appuser

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh && chown -R appuser:appuser /app

USER appuser
ENV ASPNETCORE_URLS=http://+:4000
EXPOSE 4000

ENTRYPOINT ["/app/docker-entrypoint.sh"]