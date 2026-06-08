using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace VacApp.Tests.E2ETests;

// ---------------------------------------------------------------------------
// End-to-end tests against the DEPLOYED backend (real HTTP requests).
//
// These exercise the live Azure deployment, so they are OPT-IN: by default
// they show up as "Skipped" and never run during the normal unit-test pass
// or in CI. To run them:
//
//   PowerShell:  $env:VACAPP_E2E = "1"; dotnet test --filter "Category=E2E"
//
// Optional: point them at another environment (e.g. local) with
//   $env:VACAPP_API_BASE_URL = "http://localhost:5160/api/v1"
//
// They use the pre-created account test2@gmail.com / test2 and never create
// new users, so they don't pollute the database.
// ---------------------------------------------------------------------------

/// <summary>
/// A <see cref="FactAttribute"/> that auto-skips unless the VACAPP_E2E
/// environment variable is set. Keeps live-network tests out of the default run.
/// </summary>
public sealed class E2EFactAttribute : FactAttribute
{
    public E2EFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VACAPP_E2E")))
            Skip = "E2E test. Set VACAPP_E2E=1 (and optionally VACAPP_API_BASE_URL) to run.";
    }
}

/// <summary>
/// Shared HttpClient pointed at the deployed API, plus a cached auth token so
/// every test doesn't have to sign in again.
/// </summary>
public sealed class DeployedApiFixture
{
    private const string DefaultBaseUrl =
        "https://vacapp-enfyaqdzc9bdghfq.canadacentral-01.azurewebsites.net/api/v1";

    public const string TestEmail = "test2@gmail.com";
    public const string TestPassword = "test2";

    public HttpClient Client { get; }

    private string? _cachedToken;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DeployedApiFixture()
    {
        var baseUrl = Environment.GetEnvironmentVariable("VACAPP_API_BASE_URL") ?? DefaultBaseUrl;
        Client = new HttpClient
        {
            // Trailing slash matters: relative request paths are resolved against it.
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(60) // Azure free tiers can be slow to wake up.
        };
    }

    /// <summary>Signs in once with the test account and caches the JWT.</summary>
    public async Task<string> GetAuthTokenAsync()
    {
        if (_cachedToken is not null) return _cachedToken;

        await _gate.WaitAsync();
        try
        {
            if (_cachedToken is not null) return _cachedToken;

            var response = await Client.PostAsJsonAsync(
                "user/sign-in", new { email = TestEmail, password = TestPassword });
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            _cachedToken = doc.RootElement.GetProperty("token").GetString();

            if (string.IsNullOrWhiteSpace(_cachedToken))
                throw new InvalidOperationException("Sign-in succeeded but no token was returned.");

            return _cachedToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    public HttpRequestMessage AuthorizedGet(string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}

[Trait("Category", "E2E")]
public class DeployedAuthApiTests : IClassFixture<DeployedApiFixture>
{
    private readonly DeployedApiFixture _fx;

    public DeployedAuthApiTests(DeployedApiFixture fx) => _fx = fx;

    [E2EFact]
    public async Task SignIn_WithValidCredentials_ReturnsOkAndToken()
    {
        var response = await _fx.Client.PostAsJsonAsync(
            "user/sign-in",
            new { email = DeployedApiFixture.TestEmail, password = DeployedApiFixture.TestPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [E2EFact]
    public async Task SignIn_WithWrongPassword_ReturnsClientError_NotServerError()
    {
        var response = await _fx.Client.PostAsJsonAsync(
            "user/sign-in",
            new { email = DeployedApiFixture.TestEmail, password = "definitely-the-wrong-password" });

        // Expected: a 4xx (the API should reject invalid credentials cleanly).
        // A 5xx means the backend threw an unhandled exception instead of
        // returning a proper auth error (UserCommandService throws a raw
        // Exception on bad credentials).
        Assert.InRange((int)response.StatusCode, 400, 499);
    }

    [E2EFact]
    public async Task SignUp_WithDuplicateEmail_ReturnsClientError_NotServerError()
    {
        // Reproduces the bug found manually: registering an already-existing
        // user surfaced only "Network Error" in the browser. Root cause:
        // UserCommandService.Handle(SignUpCommand) does `throw new
        // Exception("User already exists")`, the controller doesn't catch it,
        // and there is no global exception handler -> HTTP 500 whose response
        // lacks CORS headers, which the browser reports as a network error.
        //
        // Correct behaviour is a 409 Conflict (or at least a 400). This test
        // is EXPECTED TO FAIL until the backend is fixed; the failure is the
        // signal that the bug is still there.
        var response = await _fx.Client.PostAsJsonAsync(
            "user/sign-up",
            new { username = "test2", email = DeployedApiFixture.TestEmail, password = "test2" });

        Assert.InRange((int)response.StatusCode, 400, 499);
    }

    [E2EFact]
    public async Task Profile_WithoutToken_ReturnsUnauthorized()
    {
        // Correct behaviour is 401 Unauthorized. NOTE: this is EXPECTED TO FAIL
        // today because RequestAuthorizationMiddleware does `throw new
        // Exception("Null or invalid token")` for a missing/invalid token, which
        // (with no global exception handler) becomes a 500. Same root cause as
        // the duplicate-sign-up bug. A failure here means the frontend can't
        // tell "session expired" apart from a server crash.
        var response = await _fx.Client.GetAsync("user/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [E2EFact]
    public async Task Profile_WithValidToken_ReturnsOk()
    {
        var token = await _fx.GetAuthTokenAsync();

        using var request = _fx.AuthorizedGet("user/profile", token);
        var response = await _fx.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

[Trait("Category", "E2E")]
public class DeployedAiAssistantApiTests : IClassFixture<DeployedApiFixture>
{
    private readonly DeployedApiFixture _fx;

    public DeployedAiAssistantApiTests(DeployedApiFixture fx) => _fx = fx;

    [E2EFact]
    public async Task GeneralChatHistory_WithoutToken_ReturnsUnauthorized()
    {
        // Correct behaviour is 401. Like Profile_WithoutToken, this is EXPECTED
        // TO FAIL today (auth middleware throws -> 500) until the backend returns
        // proper status codes for unauthorized requests.
        var response = await _fx.Client.GetAsync("ai/general-chat");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [E2EFact]
    public async Task GeneralChatHistory_WithValidToken_DoesNotReturnServerError()
    {
        var token = await _fx.GetAuthTokenAsync();

        using var request = _fx.AuthorizedGet("ai/general-chat", token);
        var response = await _fx.Client.SendAsync(request);

        // 200 if the account has the Plus subscription and history loads;
        // 403 if [RequiresPlus] blocks a free account. Either is acceptable.
        // What we must never see is a 5xx (e.g. the empty-history path blowing up).
        Assert.True((int)response.StatusCode < 500,
            $"Unexpected server error {(int)response.StatusCode} from GET /ai/general-chat.");
    }

    [E2EFact]
    public async Task BovineChatHistory_WithValidToken_DoesNotReturnServerError()
    {
        var token = await _fx.GetAuthTokenAsync();

        // Bovine id 1 may or may not belong to this account; either way the
        // endpoint must respond cleanly (2xx/4xx), never 5xx.
        using var request = _fx.AuthorizedGet("ai/bovine-chat/1", token);
        var response = await _fx.Client.SendAsync(request);

        Assert.True((int)response.StatusCode < 500,
            $"Unexpected server error {(int)response.StatusCode} from GET /ai/bovine-chat/1.");
    }

    [E2EFact]
    public async Task BovineAnalyses_WithValidToken_DoesNotReturnServerError()
    {
        var token = await _fx.GetAuthTokenAsync();

        using var request = _fx.AuthorizedGet("ai/analyses/1", token);
        var response = await _fx.Client.SendAsync(request);

        Assert.True((int)response.StatusCode < 500,
            $"Unexpected server error {(int)response.StatusCode} from GET /ai/analyses/1.");
    }
}
