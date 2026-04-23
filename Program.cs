using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Supabase;
using System.Reflection.Metadata.Ecma335;
using System.Text;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddScoped(_ =>
            new Client(
                builder.Configuration["Supabase:Url"],
                builder.Configuration["Supabase:Key"],
                new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true,
                    Schema = "PetWise"
                }
            )
        );

        builder.Services.AddControllers();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"{builder.Configuration["Supabase:Url"]}/auth/v1";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"{builder.Configuration["Supabase:Url"]}/auth/v1",

                ValidateAudience = false, // Supabase doesn't require this
                ValidateLifetime = true
            };
        });

        builder.Services.AddAuthorization();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();   
        app.UseAuthorization();
        app.MapGet("/Health", () => Results.Ok("API is Healthy"));

        app.MapControllers();

        app.Run();
    }
}