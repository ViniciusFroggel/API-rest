## API-rest

**Este projeto é uma API REST para gerenciamento de uma barbearia, criada com foco em autenticação via JWT, roles (Funcionário / Cliente), Documentado via Swagger.**

## A aplicação está estruturada para permitir:
**Cadastro e login de usuários** (Clientes e Funcionários)
**Proteção de rotas por permissão** (roles)
**Registro de clientes, serviços e agendamentos**
**Documentação via swagger**

## 🧰 Tecnologias Utilizadas

**ASP.NET** Core	Framework da API

**Entity Framework** Core	ORM para o banco de dados

**SQL Server Local**	Banco de dados usado no ambiente local

**ASP.NET Identity**	Gerenciamento de usuários e roles

**JWT** (JSON Web Token)	Autenticação e Autorizações

## 🔐 Autenticação & Controle de Acesso

**Para Registrar um usuário (Cliente ou Funcionário):**
POST /api/Auth/register
Envie JSON:

{
  "email": "usuario@teste.com",
  "password": "Senha123!",
  "nomeCompleto": "Nome do Usuário",
  "role": "Cliente"            // ou "Funcionario"
}


Para Login:
POST /api/Auth/login
Envie:

{
  "email": "usuario@teste.com",
  "password": "Senha123!"
}


## Você receberá um token JWT. Clique no botão Authorize no Swagger, cole:

**Bearer SEU_TOKEN_AQUI**
A partir disso, rotas protegidas como POST /api/Agendamentos só funcionam se você for Funcionario ou estiver com role correta.

## 👤 Autor

**Vinícius Froggel**
**GitHub:** https://github.com/ViniciusFroggel
