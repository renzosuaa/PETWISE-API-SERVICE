using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PetWise_Application;
using PetWise_Infrastructure;
using Supabase;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        
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

       
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"{builder.Configuration["Supabase:Url"]}/auth/v1";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"{builder.Configuration["Supabase:Url"]}/auth/v1",

                ValidateAudience = false, 
                ValidateLifetime = true
            };
        });

        builder.Services.AddAuthorization();

        var app = builder.Build();

       
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