# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar solução e projeto
COPY *.sln ./
COPY SistemaBarbearia/*.csproj ./SistemaBarbearia/

# Restaurar dependências do projeto
RUN dotnet restore SistemaBarbearia/SistemaBarbearia.csproj

# Copiar todo o restante do código
COPY . ./

# Publicar apenas o projeto
RUN dotnet publish SistemaBarbearia/SistemaBarbearia.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 5000
EXPOSE 5001

ENTRYPOINT ["dotnet", "SistemaBarbearia.dll"]
