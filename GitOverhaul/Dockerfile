# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GitOverhaul.sln .
COPY GitOverhaul.Api/GitOverhaul.Api.csproj GitOverhaul.Api/
COPY GitOverhaul.Domain/GitOverhaul.Domain.csproj GitOverhaul.Domain/
COPY GitOverhaul.Infra/GitOverhaul.Infra.csproj GitOverhaul.Infra/

RUN dotnet restore

COPY GitOverhaul.Api/ GitOverhaul.Api/
COPY GitOverhaul.Domain/ GitOverhaul.Domain/
COPY GitOverhaul.Infra/ GitOverhaul.Infra/

WORKDIR /src/GitOverhaul.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot && cp /app/wwwroot/openai-actions.json /app/wwwroot/

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GitOverhaul.Api.dll"]