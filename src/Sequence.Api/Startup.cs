using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System;

namespace Sequence.Api
{
    public sealed class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new GameIdJsonConverter());
                    options.SerializerSettings.Converters.Add(new PlayerIdJsonConverter());
                    options.SerializerSettings.Converters.Add(new StringEnumConverter(camelCaseText: true));
                    options.SerializerSettings.Converters.Add(new TileJsonConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddHealthChecks();
            services.AddSequence(_configuration);

            if (_env.IsDevelopment())
            {
                services.AddCors();
            }
        }

        public void Configure(IApplicationBuilder app)
        {
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
            app.UseMvc();
        }
    }
}
