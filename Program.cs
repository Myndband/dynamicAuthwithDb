global using Microsoft.EntityFrameworkCore;
global using DotnetAuthAndFileHandling.Models;
global using DotnetAuthAndFileHandling.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using DotnetAuthAndFileHandling.Interface;
using DotnetAuthAndFileHandling.Service;
using DocumentFormat.OpenXml.InkML;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase")));
builder.Services.AddCors(option =>
{
    option.AddPolicy("cors", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryverysecret.....")),

        ValidateAudience = false,
        ValidateIssuer = false,

    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

//app.UseHttpsRedirection();
//pipeline
app.UseCors("cors");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
