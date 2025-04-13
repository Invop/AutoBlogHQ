using AutoBlogHQ.API;
using AutoBlogHQ.Application.Database;
using AutoBlogHQ.Application.Models;
using Microsoft.AspNetCore.Identity;
using AutoBlogHQ.Application;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

builder.Services.AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");
app.MapGroup("/identity").MapIdentityApi<ApplicationUser>();
app.MapPost("/identity/logout", async (SignInManager<ApplicationUser> signInManager,
        [FromBody] object? empty) =>
    {
        if (empty == null) return Results.Unauthorized();
        await signInManager.SignOutAsync();
        return Results.Ok();
    })
    .WithOpenApi()
    .RequireAuthorization();
app.Run();