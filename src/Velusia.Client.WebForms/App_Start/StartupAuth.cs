using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace Velusia.Client.WebForms
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            var scope = $"{OpenIdConnectScope.OpenId} {OpenIdConnectScope.OpenIdProfile} {OpenIdConnectScope.Email} roles";

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44313/",

                ClientId = "Velusia.Client.WebForms",
                ClientSecret = "D8FC8E80-266C-4513-B7A5-6D74742E1A5A",

                RedirectUri = "https://localhost:44376/signin-oidc",
                PostLogoutRedirectUri = "https://localhost:44376/signout-callback-oidc",

                RedeemCode = true,

                ResponseMode = OpenIdConnectResponseMode.Query,
                ResponseType = OpenIdConnectResponseType.Code,

                Scope = "openid profile email roles",

                SecurityTokenValidator = new JwtSecurityTokenHandler
                {
                    // Disable the built-in JWT claims mapping feature.
                    InboundClaimTypeMap = new Dictionary<string, string>(),
                    // MapInboundClaims = false,
                },

                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                    ValidateAudience = true,
                    ValidateIssuer = true,
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // Note: by default, the OIDC client throws an OpenIdConnectProtocolException
                    // when an error occurred during the authentication/authorization process.
                    // To prevent a YSOD from being displayed, the response is declared as handled.
                    AuthenticationFailed = notification =>
                    {
                        if (string.Equals(notification.ProtocolMessage.Error, "access_denied", StringComparison.Ordinal))
                        {
                            notification.HandleResponse();

                            notification.Response.Redirect("/");
                        }

                        return Task.CompletedTask;
                    },
                    SecurityTokenValidated = notification =>
                    {
                        return Task.CompletedTask;
                    },
                    TokenResponseReceived = notification =>
                    {
                        return Task.CompletedTask;
                    },
                    AuthorizationCodeReceived = notification =>
                    {
                        return Task.CompletedTask;
                    },
                    MessageReceived = notification =>
                    {
                        return Task.CompletedTask;
                    },
                    SecurityTokenReceived = notification =>
                    {
                        return Task.CompletedTask;
                    }
                }
            });

            // This makes any middleware defined above this line run before the Authorization rule is applied in web.config
            app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}
