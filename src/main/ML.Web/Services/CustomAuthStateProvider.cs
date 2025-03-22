using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ML.Web.Services;


public class CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient) : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly HttpClient _httpClient = httpClient;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorage.GetItemAsync<string>("accessToken");

        if (string.IsNullOrWhiteSpace(savedToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", savedToken);

        ClaimsIdentity identity = new(ParseClaimsFromJwt(savedToken), "jwt");
        ClaimsPrincipal user = new(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication()
    {
        ClaimsPrincipal authenticatedUser = new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "User")], "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        ClaimsPrincipal anonymousUser = new(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private static List<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            JsonWebTokenHandler handler = new();
            JsonWebToken jsonToken = handler.ReadJsonWebToken(jwt);
            List<Claim> claims = jsonToken.Claims.ToList();

            if (!claims.Any(c => c.Type == ClaimTypes.Role))
            {
                if (jsonToken.TryGetPayloadValue("role", out object roleValue) && roleValue != null)
                {
                    HandleRoleValue(roleValue, claims);
                }
                
                if (jsonToken.TryGetPayloadValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out object msRoleValue) && msRoleValue != null)
                {
                    HandleRoleValue(msRoleValue, claims);
                }
            }

            return claims;
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error parsing JWT: {ex.Message}");
            return [];
        }
    }
    private static void HandleRoleValue(object roleValue, List<Claim> claims)
    {
        if (roleValue is JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    string? role = jsonElement.GetString();
                    if (!string.IsNullOrEmpty(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    break;
                    
                case JsonValueKind.Array:
                    foreach (var element in jsonElement.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            string? rolee = element.GetString();
                            if (!string.IsNullOrEmpty(rolee))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, rolee));
                            }
                        }
                    }
                    break;
            }
        }
        else
        {
            var roleString = roleValue.ToString();
            if (!string.IsNullOrEmpty(roleString))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleString));
            }
        }
    }
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}