using AutoBlogHQ.API;
using AutoBlogHQ.API.Mapping;
using AutoBlogHQ.Application;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddMailer()
    .AddCustomAuthenticationAndAuthorization()
    .AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException())
    .AddOpenApiConfiguration()
    .AddValidators()
    .AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.ApplyMigrations();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();
app.MapFallbackToFile("/index.html");
await app.CreateDefaultAuthorAsync(builder.Configuration);

app.Run();