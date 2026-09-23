using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Petit.WebApi.Data;

namespace Petit.WepApi.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _databaseName = $"PetDb_Test_{Guid.NewGuid():N}";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Set environment to "Testing" so Program.cs uses InMemory database instead of Npgsql
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var source = new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["ConnectionStrings:PetDbConnection"] = "",
                    ["Keycloak:Authority"] = "http://localhost:test",
                    ["Keycloak:Audience"] = "test-audience"
                }
            };
            configBuilder.Add(source);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the default JWT authentication scheme and replace with a test-friendly one
            var jwtDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.IAuthenticationHandlerProvider));

            if (jwtDescriptor != null)
            {
                services.Remove(jwtDescriptor);
            }

            // Add a fake authentication handler that auto-authenticates all requests
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Remove all DbContextOptions<PetDbContext> registrations (from Program.cs)
            var dbContextOptionsDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<PetDbContext>))
                .ToList();

            foreach (var descriptor in dbContextOptionsDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove the PetDbContext itself if it's registered
            var dbContextDescriptors = services
                .Where(d => d.ImplementationType == typeof(PetDbContext))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Build options first - use a static database name so all contexts share the same InMemory store
            var optionsBuilder = new DbContextOptionsBuilder<PetDbContext>();
            optionsBuilder.UseInMemoryDatabase("__PetDb_Test__");
            var sharedOptions = optionsBuilder.Options;

            // Register options as singleton
            services.AddSingleton<DbContextOptions<PetDbContext>>(sharedOptions);

            // Create a single context instance and register it as singleton only.
            // The container won't track it for disposal, preventing ObjectDisposedException.
            // All scopes and the HTTP pipeline will resolve this same instance via the singleton options.
            var sharedContext = new PetDbContext(sharedOptions);
            services.AddSingleton<PetDbContext>(sharedContext);
        });

        return base.CreateHost(builder);
    }

    public void DisposeTestDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetDbContext>();
        dbContext.Database.EnsureDeleted();
    }

    public new void Dispose()
    {
        try
        {
            DisposeTestDatabase();
        }
        catch
        {
            // Ignore disposal errors
        }
    }
}

// A test authentication handler that auto-authenticates all requests with a fake "admin" user
public class TestAuthHandler : AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock) 
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "test-user"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "test-user")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
