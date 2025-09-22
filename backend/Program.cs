using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapUsersEndpoints();

app.Run();
