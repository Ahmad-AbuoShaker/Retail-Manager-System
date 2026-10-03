
using Application.Interface;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;


namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Database


            builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"))
                .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
               

                if (builder.Environment.IsDevelopment())
                    options.EnableSensitiveDataLogging();

            });

            builder.Services.AddScoped<AppDbContext>(sp => (AppDbContext)sp.GetRequiredService<IAppDbContext>());


            #endregion

            #region DI

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
            #endregion

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


            var app = builder.Build();


            #region Middleware
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("UniAPICORSPolicy");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            #endregion


            app.Run();
        }
    }
}
