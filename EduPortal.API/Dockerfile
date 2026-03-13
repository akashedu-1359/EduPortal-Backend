# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EduPortal.API/EduPortal.API.csproj", "EduPortal.API/"]
COPY ["EduPortal.Application/EduPortal.Application.csproj", "EduPortal.Application/"]
COPY ["EduPortal.Domain/EduPortal.Domain.csproj", "EduPortal.Domain/"]
COPY ["EduPortal.Infrastructure/EduPortal.Infrastructure.csproj", "EduPortal.Infrastructure/"]

RUN dotnet restore "EduPortal.API/EduPortal.API.csproj"

COPY . .
WORKDIR "/src/EduPortal.API"
RUN dotnet publish "EduPortal.API.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "EduPortal.API.dll"]
