//using Agora.API.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Agora.API
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder ConfigureAgoraAPI(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors();
            builder.Services.AddFastEndpoints();
            builder.Services.AddResponseCaching();
            //builder.Services.AddTransient<AccessTokenService>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
                            {
                                c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
                                c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                                {
                                    ValidAudience = builder.Configuration["Auth0:Audience"],
                                    ValidIssuer = $"https://{builder.Configuration["Auth0:Domain"]}",
                                    ValidateLifetime = true
                                };
                            });

            return builder;
        }

        public static WebApplication ConfigureApiApplication(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseCors(x => x.WithOrigins(app.Configuration["Endpoints:WebApp"]!)
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials()
                               .WithExposedHeaders("Access-Control-Allow-Origin")); 
            app.UseResponseCaching();
            app.UseFastEndpoints();
            app.UseAuthorization();
            
            app.Urls.Add(app.Configuration["Endpoints:WebApi"]!);

            return app;
        }
    }
}
