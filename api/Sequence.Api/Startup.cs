using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace Sequence.Api
{
    public sealed class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(options => ConfigureJsonSerializerSettings(options.SerializerSettings))
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services
                .AddSignalR()
                .AddJsonProtocol(options => ConfigureJsonSerializerSettings(options.PayloadSerializerSettings));

            services.AddHealthChecks();
            services.AddMemoryCache();
            services.AddSequence(_configuration, _loggerFactory);

            if (_env.IsDevelopment())
            {
                services.AddCors();
            }

            services
                .AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.ApiName = "api";
                    options.Authority = "http://localhost:5001";
                    options.RequireHttpsMetadata = false;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            if (_env.IsProduction())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }
            else if (_env.IsDevelopment())
            {
                app.UseCors(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:3000")
                    .AllowCredentials());

                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health");
            app.UseSignalR(builder => builder.MapHub<GameHub>("/game-hub"));
            app.UseMvc();
        }

        private static void ConfigureJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new GameIdJsonConverter());
            settings.Converters.Add(new PlayerHandleJsonConverter());
            settings.Converters.Add(new PlayerIdJsonConverter());
            settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            settings.Converters.Add(new TileJsonConverter());
        }
    }
}
