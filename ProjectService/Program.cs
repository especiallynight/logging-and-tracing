using Microsoft.EntityFrameworkCore;
using ProjectService.Data;
using ProjectService.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "project_management")));

builder.Services.AddHttpClient<UserClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5135/");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();