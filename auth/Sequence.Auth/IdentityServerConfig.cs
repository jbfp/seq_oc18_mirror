using IdentityServer4.Models;
using System.Collections.Immutable;

namespace Sequence.Auth
{
    internal static class IdentityServerConfig
    {
        public static IImmutableList<ApiResource> ApiResources { get; } = ImmutableList.Create(
            new ApiResource("api", "API"),
            new ApiResource("test", "Test")
        );

        public static IImmutableList<Client> Clients { get; } = ImmutableList.Create(
            new Client
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "api" },
                ClientId = "my-client",
                ClientSecrets = { new Secret("my-secret".Sha256()) },
            },
            new Client
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "test" },
                ClientId = "test-client",
                ClientSecrets = { new Secret("test-secret".Sha256()) },
            }
        );
    }
}
