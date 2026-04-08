
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureAPIsPractice.Data;
using SecureAPIsPractice.Interfaces;
using SecureAPIsPractice.Models;
using SecureAPIsPractice.Services;
using System.Text;

namespace SecureAPIsPractice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddAutoMapper(typeof(Program));
            // Bind JWT settings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            //add identity service
            builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "sub", 
                   // RoleClaimType = "roles"

                };
            });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseSqlServer(
                  builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            #region ForDebuging

            //app.Use(async (context, next) =>
            //{
            //    var token = context.Request.Headers["Authorization"].ToString();
            //    Console.WriteLine($"Token: {token}");

            //    await next();

            //    Console.WriteLine($"Status Code: {context.Response.StatusCode}");
            //});

            //app.Use(async (context, next) =>
            //{
            //    await next();

            //    if (context.User.Identity.IsAuthenticated)
            //    {
            //        foreach (var claim in context.User.Claims)
            //        {
            //            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("User is NOT authenticated");
            //    }
            //});

            //app.Use(async (context, next) =>
            //{
            //    await next();

            //    if (context.User.Identity.IsAuthenticated)
            //    {
            //        foreach (var claim in context.User.Claims)
            //        {
            //            Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("User is NOT authenticated");
            //    }
            //});


            #endregion


            app.UseAuthorization();


            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
