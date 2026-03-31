using Asp.Versioning;
using Microsoft.OpenApi;
using Serilog;
using Voltiq.API.ExceptionHandlers;
using Voltiq.Application;
using Voltiq.Infrastructure;
using Voltiq.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi("v1", o =>
{
    o.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Voltiq API",
            Description = "API da aplicação Voltiq",
            Version = "v1",
            License = new OpenApiLicense
            {
                Name = "GNU AFFERO GENERAL PUBLIC LICENSE Version 3 (AGPL-3.0)",
                Url = new Uri("https://www.gnu.org/licenses/agpl-3.0.pt-br.html")
            },
            Summary =
                """
                Voltiq é uma aplicação para gerir orçamentos, clientes e materiais para
                profissionais autônomos, principalmente eletricistas, oferecendo uma maneira
                prática e eficiente de gerir seus clientes e orçamentos.
                """,
            Contact = new OpenApiContact
            {
                Name = "Team Voltiq",
                Email = "suporte@voltiq.com.br"
            }
        };

        document.Servers =
        [
            new OpenApiServer { Url = "https://localhost:7085/", Description = "Servidor Local" }
        ];

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Autenticação via _token_ JWT. Insira-o no campo abaixo"
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                []
            }
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    await DatabaseMigration.ApplyAsync(app.Services);

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/docs/{documentName}.json");
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/docs/v1.json", "Voltiq API - v1"); });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

await app.RunAsync();
