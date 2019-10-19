using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sequence.AspNetCore;
using Sequence.RealTime;
using System;

namespace Sequence
{
    public sealed class Startup : IStartup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                ConfigureJsonSerializerSettings(settings);
                return settings;
            };

            services
                .AddMvc()
                .AddJsonOptions(options => ConfigureJsonSerializerSettings(options.SerializerSettings))
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services
                .AddSignalR()
                .AddJsonProtocol(options => ConfigureJsonSerializerSettings(options.PayloadSerializerSettings));

            services.AddHealthChecks();
            services.AddMemoryCache();

            if (_env.IsDevelopment())
            {
                services.AddCors();
            }

            services.AddSequence(_configuration);

            return services.BuildServiceProvider();
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
            settings.Converters.Add(new GameEventConverter());
            settings.Converters.Add(new GameIdJsonConverter());
            settings.Converters.Add(new PlayerHandleJsonConverter());
            settings.Converters.Add(new PlayerIdJsonConverter());
            settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            settings.Converters.Add(new TileJsonConverter());
        }
    }
}
