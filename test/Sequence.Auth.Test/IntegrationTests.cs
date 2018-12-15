using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Sequence.Auth.Test
{
    public sealed class IntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string _clientId = "test-client";
        private const string _clientSecret = "test-secret";
        private const string _scope = "test";

        private readonly WebApplicationFactory<Startup> _factory;

        public IntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        [Fact]
        public async Task CanGetDiscoveryDocument()
        {
            var client = _factory.CreateClient();

            var discovery = await client.GetDiscoveryDocumentAsync();

            Assert.False(discovery.IsError, discovery.Error);
        }

        [Fact]
        public async Task CanRequestClientCredentialsToken()
        {
            var client = _factory.CreateClient();
            var discovery = await client.GetDiscoveryDocumentAsync();
            var request = new ClientCredentialsTokenRequest
            {
                Address = discovery.TokenEndpoint,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scope = _scope,
            };

            var token = await client.RequestClientCredentialsTokenAsync(request);

            Assert.False(token.IsError, token.Error);
        }
    }
}
