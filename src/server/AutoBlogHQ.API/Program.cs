using AutoBlogHQ.API;
using AutoBlogHQ.API.Auth;
using AutoBlogHQ.API.Endpoints;
using AutoBlogHQ.Application;
using AutoBlogHQ.Application.Database;
using AutoBlogHQ.Application.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PasswordlessLoginTokenProviderOptions>(
    x => x.TokenLifespan = TimeSpan.FromMinutes(5));

builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy(AuthConstants.AdminUserPolicyName,
        p => p.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
});
builder.Services.Configure<IdentityOptions>(options => { options.SignIn.RequireConfirmedEmail = false; });

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints()
    .AddPasswordlessLoginTotpTokenProvider();

// builder.Services.AddTransient(typeof(IEmailSender<>), typeof(CustomMessageEmailSender<>));
// builder.Services.AddTransient<IEmailSender, LoggingEmailSender>();

builder.Services.AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection") ??
                             throw new InvalidOperationException());

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
}

app.ApplyMigrations();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapFallbackToFile("/index.html");
await app.CreateDefaultAuthorAsync(builder.Configuration);


app.MapApiEndpoints();
app.Run();