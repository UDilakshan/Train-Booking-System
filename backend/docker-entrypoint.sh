#!/bin/sh
set -e

echo "Applying migrations and seeding reference data..."
dotnet RailwayReservation.Api.dll seed

echo "Starting API..."
exec dotnet RailwayReservation.Api.dll
