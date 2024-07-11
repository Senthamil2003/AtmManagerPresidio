using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using AtmApplicationApp.Context;
// using AtmApplicationApp.Interfaces.Repositories;
// using AtmApplicationApp.Interfaces.Services;
// using AtmApplicationApp.Mappings;
// using AtmApplicationApp.Models;
// using AtmApplicationApp.Repositories;
// using AtmApplicationApp.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WatchDog;

using WatchDog.src.Models;
using AtmApplicationApp.Models;
using AtmApplicationApp.Repository;
using AtmApplicationApp.Interface;
using AtmApplicationApp.Services;

namespace AtmApplicationApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            #region Context
            builder.Services.AddDbContext<AtmContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"))
            );
            #endregion
            #region JWT-Authentication-Injection
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenKey:JWT"])),
                    };
                });
            #endregion

            #region Repositories
            builder.Services.AddScoped<IRepository<long, Account>, AccountRepository>();
            builder.Services.AddScoped<IRepository<long, Card>, CardRepository>();
            builder.Services.AddScoped<IRepository<int, Customer>, CustomerRepository>();
            builder.Services.AddScoped<IRepository<int, Transaction>, TransactionRepository>();
            #endregion

            #region Services
            builder.Services.AddScoped<IAtmService, AtmService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            #endregion


            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
              #region Cors
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("*")
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    });
            });
            #endregion
            #region Swagger
            builder.Services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } } });
            });
            #endregion

            #region Logging
            
            builder.Services.AddWatchDogServices(opt =>
            {
                opt.SetExternalDbConnString = builder.Configuration.GetConnectionString("WatchDogConnection");
                opt.DbDriverOption = WatchDog.src.Enums.WatchDogDbDriverEnum.MSSQL;
            });

            #endregion

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("AllowSpecificOrigin");

            app.MapControllers();

            // inject middleware for WatchDog
            app.UseWatchDogExceptionLogger();

            var watchdogCredentials = builder.Configuration.GetSection("WatchDog");
            app.UseWatchDog(opt =>
            {
                opt.WatchPageUsername = watchdogCredentials["username"];
                opt.WatchPagePassword = watchdogCredentials["password"];
            });

            #region Run
            //Setup local IP\
            static string LocalIPAddress()
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                    {
                        return endPoint.Address.ToString();
                    }
                    else
                    {
                        return "127.0.0.1";
                    }
                }
            }
            string localIP = LocalIPAddress();
            //app.Urls.Add("http://" + localIP + ":5072");
            //app.Urls.Add("https://" + localIP + ":7072");
            app.Run();
            #endregion
        }
    }
}
