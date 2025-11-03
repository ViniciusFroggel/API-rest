using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaBarbearia.Data;
using SistemaBarbearia.Models;
using SistemaBarbearia.Enums;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Database --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Identity --------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // LOGIN POR TELEFONE
    options.User.RequireUniqueEmail = false; // email não é obrigatório
    options.SignIn.RequireConfirmedPhoneNumber = true; // exigir confirmação do telefone

    // Regras de senha
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // necessário para token via telefone

// -------------------- JWT Authentication --------------------
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// -------------------- Swagger --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Digite: Bearer {seu token aqui}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -------------------- Controllers --------------------
builder.Services.AddControllers();

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -------------------- Seed Roles & Admin --------------------
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { UserRoles.Cliente, UserRoles.Funcionario, UserRoles.Admin, UserRoles.AdminMaster };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Criar AdminMaster padrão
    var adminPhone = "41998431178"; // DDD + número
    var masterUser = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == adminPhone);

    if (masterUser == null)
    {
        masterUser = new ApplicationUser
        {
            UserName = adminPhone,
            PhoneNumber = adminPhone,
            PhoneNumberConfirmed = true, // confirmar automaticamente o admin
            NomeCompleto = "Administrador Master",
            TipoUsuario = UserRoles.AdminMaster,
            Nivel = UserLevels.AdminMaster
        };

        var result = await userManager.CreateAsync(masterUser, "Master123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(masterUser, UserRoles.AdminMaster);
        }
    }
}

// -------------------- Apply Migrations --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
