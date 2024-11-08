using EventVault.Data;
using EventVault.Data.Repositories;
using EventVault.Data.Repositories.IRepositories;
using EventVault.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotNetEnv;
using EventVault.Models;
using EventVault.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EventVault
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<EventVaultDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationContext")));


            //CORS-Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("LocalReact", policy =>
                {
                    //l�gg in localhost reactapp som k�r n�r vi startar react. 
                    policy.WithOrigins("http://localhost:5174")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });


            //Policy som �r mindre s�ker och till�ter vem som helst att ansluta. Om god s�kerhet finns i api med auth, s� kan den h�r anv�ndas.
            //builder.Services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(policy =>
            //    {
            //        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            //    });
            //});

            // Configure SMTP settings
            // (The correct one) :)
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();


            // Identity framework
            builder.Services.AddAuthorization();
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<EventVaultDbContext>()
                .AddDefaultTokenProviders();

            // JWT Authentication
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            // Other services
            builder.Services.AddControllers();
          
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Services & repositories events
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IEventServices, EventServices>();
            builder.Services.AddScoped<IKBEventServices, KBEventServices>();
            builder.Services.AddScoped<IVisitStockholmServices, VisitStockholmServices>();
            builder.Services.AddScoped<ITicketMasterServices, TicketMasterServices>();
            builder.Services.AddHttpClient<IEventbriteServices, EventbriteServices>();

            // Services & repositories identity
            builder.Services.AddTransient<IAuthServices, AuthServices>();
            builder.Services.AddTransient<IRoleServices, RoleServices>();
            builder.Services.AddTransient<IAdminServices, AdminServices>();

            // User repo & service
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            //friendship
            builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            builder.Services.AddScoped<IFriendshipService, FriendshipService>();

            var app = builder.Build();


            // Use corspolicy set above ^.
            app.UseCors("LocalReact");

            using (var scope = app.Services.CreateScope())
            {
                var roleService = scope.ServiceProvider.GetRequiredService<IRoleServices>();
                await roleService.InitalizeRolesAsync();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

