using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace DigitalTriageApp.Helpers
{
    /// <summary>
    /// Helper class to work with antiforgery tokens in Blazor components
    /// </summary>
    public class AntiforgeryHelper
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IAntiforgery _antiforgery;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AntiforgeryHelper(
            IJSRuntime jsRuntime,
            IAntiforgery antiforgery,
            IHttpContextAccessor httpContextAccessor)
        {
            _jsRuntime = jsRuntime;
            _antiforgery = antiforgery;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the antiforgery token value
        /// </summary>
        public string GetToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(_httpContextAccessor.HttpContext!);
            return tokens.RequestToken!;
        }

        /// <summary>
        /// Gets the antiforgery token asynchronously from JavaScript
        /// </summary>
        public async Task<string?> GetTokenFromJavaScriptAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("BlazorAntiforgery.getToken");
                return token;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fetches the antiforgery token from the server endpoint
        /// </summary>
        public async Task<string?> FetchTokenAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("BlazorAntiforgery.fetchToken");
                return token;
            }
            catch
            {
                return null;
            }
        }
    }
}

