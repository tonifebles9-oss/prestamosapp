using PrestamosApp.Application.Common.Interfaces;
using PrestamosApp.Infrastructure.Installments;
using PrestamosApp.Application.Installments.Repositories;
using PrestamosApp.Infrastructure.Loans;
using PrestamosApp.Application.Loans.Services;
using PrestamosApp.Application.Loans.Repositories;
using Microsoft.EntityFrameworkCore;
using PrestamosApp.Application.Clients.Repositories;
using PrestamosApp.Application.Clients.Services;
using PrestamosApp.Infrastructure.Clients;
using PrestamosApp.Infrastructure.Data;
using PrestamosApp.Application.Payments.Repositories;
using PrestamosApp.Application.Payments.Services;
using PrestamosApp.Infrastructure.Payments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var frontendUrl = builder.Configuration["FRONTEND_URL"];

var allowedOrigins = new List<string>
{
    "http://localhost:5173",
    "http://localhost:5174"
};

if (!string.IsNullOrWhiteSpace(frontendUrl))
{
    allowedOrigins.Add(frontendUrl.TrimEnd('/'));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta configurar ConnectionStrings__DefaultConnection.");

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ClientService>();

builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<LoanService>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<PaymentService>();

builder.Services.AddScoped<IInstallmentRepository, InstallmentRepository>();

var app = builder.Build();

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    Application = "PrestamosApp",
    Status = "Running"
}));

app.Run();
