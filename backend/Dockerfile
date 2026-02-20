# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + all csproj files first (layer-cache friendly)
COPY PostTradeBackoffice.sln ./
COPY src/Core/PostTrade.Domain/PostTrade.Domain.csproj             src/Core/PostTrade.Domain/
COPY src/Core/PostTrade.Application/PostTrade.Application.csproj   src/Core/PostTrade.Application/
COPY src/Infrastructure/PostTrade.Persistence/PostTrade.Persistence.csproj     src/Infrastructure/PostTrade.Persistence/
COPY src/Infrastructure/PostTrade.Infrastructure/PostTrade.Infrastructure.csproj src/Infrastructure/PostTrade.Infrastructure/
COPY src/Presentation/PostTrade.API/PostTrade.API.csproj            src/Presentation/PostTrade.API/

# Restore dependencies (cached unless csproj/sln changes)
RUN dotnet restore src/Presentation/PostTrade.API/PostTrade.API.csproj

# Copy the rest of the source
COPY src/ src/

# Publish release build
RUN dotnet publish src/Presentation/PostTrade.API/PostTrade.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Run as non-root
USER appuser

# .NET listens on 8080 by default in containers (ASPNETCORE_URLS)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PostTrade.API.dll"]
