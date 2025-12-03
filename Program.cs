using CarteiraDB.Persistence;
using CarteiraDB.Persistence.Repository;
using CarteiraDB.Service;
using CarteiraDB.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CarteiraService>();
builder.Services.AddScoped<SaldoService>();
builder.Services.AddScoped<MoedaService>();
builder.Services.AddScoped<Deposito_SaqueService>();
builder.Services.AddScoped<ConversaoService>();
builder.Services.AddScoped<TransferenciaService>();

builder.Services.AddSwaggerGen(
        sw =>
        {
            sw.SwaggerDoc("v1",
                           new OpenApiInfo { Title = "Projeto Carteira API", Version = "v1" });

            // configuração de "habilitação de uso do JWT
            sw.AddSecurityDefinition(
                    "Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Digite 'Bearer' - sem as aspas - [espaço] e digite/cole seu token JWT.\n\nExemplo: Bearer 12345678910abasdfgrest#$@"
                    });

            sw.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType
                                            .SecurityScheme,
                                       Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    }
                );
        }
    );



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.Configure<DB>(
    builder.Configuration.GetSection("ConnectionStrings"));

// Register repositories
builder.Services.AddScoped<CarteiraRepository>(provider =>
{
    // CarteiraRepository expects the raw connection string
    return new CarteiraRepository(connectionString);
});

builder.Services.AddScoped<MoedaRepository>(provider =>
{
    // MoedaRepository expects the raw connection string
    return new MoedaRepository(connectionString);
});

builder.Services.AddScoped<SaldoRepository>(provider =>
{
    // SaldoRepository expects IConfiguration
    return new SaldoRepository(provider.GetRequiredService<IConfiguration>());
});

builder.Services.AddScoped<DepositoRepository>(provider =>
{
    // DepositoRepository depends on IConfiguration and CarteiraRepository
    return new DepositoRepository(
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<CarteiraRepository>());
});

builder.Services.AddScoped<ConversaoRepository>(provider =>
{
    // ConversaoRepository depends on IConfiguration, MoedaRepository and SaldoRepository
    return new ConversaoRepository(
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<MoedaRepository>(),
        provider.GetRequiredService<SaldoRepository>());
});

builder.Services.AddScoped<TransferenciaRepository>(provider =>
{
    // TransferenciaRepository depends on IConfiguration, MoedaRepository, CarteiraRepository and SaldoRepository
    return new TransferenciaRepository(
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<MoedaRepository>(),
        provider.GetRequiredService<CarteiraRepository>(),
        provider.GetRequiredService<SaldoRepository>());
});



var app = builder.Build();





// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // configurar a "abertura" da tela do Swagger quando executarmos a aplicação
    app.UseSwaggerUI(
           s =>
           {
               s.SwaggerEndpoint("/swagger/v1/swagger.json", "API Carteira v1");
               s.RoutePrefix = "";
           }
        );

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
