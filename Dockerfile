# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solução
COPY *.sln ./

# Criar pasta para os projetos e copiar csproj
RUN mkdir SistemaBarbearia
COPY *.csproj ./SistemaBarbearia/

# Restaurar dependências
WORKDIR /src/SistemaBarbearia
RUN dotnet restore

# Copiar todo o restante do código
COPY . ./

# Publicar em Release
RUN dotnet publish -c Release -o /app

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "SistemaBarbearia.dll"]
