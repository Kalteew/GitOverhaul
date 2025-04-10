# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GitOverhaul.sln .
COPY GitOverhaul.Api/GitOverhaul.Api.csproj GitOverhaul.Api/
COPY GitOverhaul.Domain/GitOverhaul.Domain.csproj GitOverhaul.Domain/
COPY GitOverhaul.Infra/GitOverhaul.Infra.csproj GitOverhaul.Infra/
COPY GitOverhaul.SchemaGen/GitOverhaul.SchemaGen.csproj GitOverhaul.SchemaGen/

RUN dotnet restore

COPY GitOverhaul.Api/ GitOverhaul.Api/
COPY GitOverhaul.Domain/ GitOverhaul.Domain/
COPY GitOverhaul.Infra/ GitOverhaul.Infra/
COPY GitOverhaul.SchemaGen/ GitOverhaul.SchemaGen/

# Generate OpenAI schema during build
WORKDIR /src
RUN dotnet run --project GitOverhaul.SchemaGen

# Publish main API project
WORKDIR /src/GitOverhaul.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GitOverhaul.Api.dll"]