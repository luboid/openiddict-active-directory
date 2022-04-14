using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Velusia.Server.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Velusia.Server;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var configs = new[]
        {
            new OpenIddictApplicationDescriptor
            {
                ClientId = "mvc",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "MVC client application",
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:44338/signout-callback-oidc")
                },
                RedirectUris =
                {
                    new Uri("https://localhost:44338/signin-oidc")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            },
            new OpenIddictApplicationDescriptor
            {
                ClientId = "Velusia.Client.WebForms",
                ClientSecret = "D8FC8E80-266C-4513-B7A5-6D74742E1A5A",
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "Velusia.Client.WebForms",
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:44376/Account/SignOut"),
                },
                RedirectUris =
                {
                    new Uri("https://localhost:44376/signin-oidc")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            },
            new OpenIddictApplicationDescriptor
            {
                ClientId = "tan",
                ClientSecret = "2F6418A5-FAC3-4FA9-9B55-4FBB2BBE645E",
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "Internet Banking backoffice app",
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:44393/?logout=yes")
                },
                RedirectUris =
                {
                    new Uri("https://localhost:44393/signin-oidc")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            },
        };

        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        foreach (var config in configs)
        {
            if (await manager.FindByClientIdAsync(config.ClientId) == null)
            {
                await manager.CreateAsync(config);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
