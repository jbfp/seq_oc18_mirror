using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sequence.Auth
{
    public sealed class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            var identityServerBuilder = services
                .AddIdentityServer()
                .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
                .AddInMemoryClients(IdentityServerConfig.Clients);

            if (_env.IsDevelopment())
            {
                identityServerBuilder.AddDeveloperSigningCredential();

                services.AddCors();
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseIdentityServer();
        }
    }
}
