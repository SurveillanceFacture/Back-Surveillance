using Surveillance_API.Interfaces;
using Surveillance_API.Services.Engine;
using Surveillance_API.Services.Regras;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddScoped<IConciliacaoRegra, ValorUnitarioRegra>();
builder.Services.AddScoped<IConciliacaoRegra, QuantidadeRegra>();
builder.Services.AddScoped<IConciliacaoRegra, NcmRegra>();

builder.Services.AddScoped<ConciliacaoEngine>();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
