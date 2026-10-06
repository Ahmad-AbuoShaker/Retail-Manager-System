using Application;
using Application.Interfaces;
using Application.Interface.Data;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using WebApi.Services;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Database

             string? ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            

            builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
            {
                options.UseSqlServer(ConnectionString).LogTo(Console.WriteLine,
                                      new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
                
                if (builder.Environment.IsDevelopment())
                    options.EnableSensitiveDataLogging();

            });

            builder.Services.AddScoped<AppDbContext>(sp => (AppDbContext)sp.GetRequiredService<IAppDbContext>());


            #endregion

            #region DI

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.Scan(scan => scan.FromAssemblyOf<Application.IAssemblyMaker>()
                            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
                            .AsMatchingInterface()
                            .WithScopedLifetime()
                            );

            builder.Services.Scan(scan => scan.FromAssemblyOf<Infrastructure.IAssemblyMaker>()
                            .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
                            .AsMatchingInterface()
                            .WithScopedLifetime()
                            );

            builder.Services.AddValidatorsFromAssemblyContaining<Application.IAssemblyMaker>();
            #endregion

            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is missing. Set 'Jwt:Key' in user-secrets or configuration.");
            }

            var taxSection = builder.Configuration.GetSection(TaxSettings.SectionName);
            if (!taxSection.Exists())
            {
                throw new InvalidOperationException("Tax configuration section is missing.");
            }

            builder.Services.AddOptions<TaxSettings>()
                .Bind(taxSection)
                .Validate(s => s.Rate >= 0 && s.Rate <= 1, "Tax rate must be between 0 and 1.")
                .ValidateOnStart();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });
            

        

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            #region Swagger

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Training Center System API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter your JWT token only",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            #endregion
          
            
            //builder.Services.AddRateLimiter(options =>
            //{
            //    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            //    options.AddFixedWindowLimiter("auth", o =>
            //    {
            //        o.PermitLimit = 5;
            //        o.Window = TimeSpan.FromMinutes(1);
            //        o.QueueLimit = 0;
            //    });
            //});

            var app = builder.Build();


            #region Middleware
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

           
          

            app.UseCors("UniAPICORSPolicy");
            //app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            #endregion


            app.Run();
        }
    }
}