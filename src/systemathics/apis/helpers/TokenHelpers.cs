// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenHelpers.cs" company="Systemathics SAS">
//   Copyright (c) Systemathics (rd@systemathics.com)
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Systemathics.Apis.Helpers
{
    #region Usings

    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;

    #endregion

    /// <summary>
    /// Helps with tokens.
    /// </summary>
    public static class TokenHelpers
    {
        #region Constants

        /// <summary>
        /// The default audience.
        /// </summary>
        private const string DefaultAudience = "https://prod.ganymede-prod";

        /// <summary>
        /// The default tenant.
        /// </summary>
        private const string DefaultTenant = "ganymede-prod.eu.auth0.com";

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a JWT Authorization token suitable to call Ganymede gRPC APIs.
        /// We either use 'AUTH0_TOKEN' environment variable (if present) to create a bearer token from it.
        /// Or 'CLIENT_ID' and 'CLIENT_SECRET' environment variables (optionally 'AUDIENCE' if <see cref="DefaultAudience"/> is not what you need, and 'TENANT' if default value <see cref="DefaultTenant"/> must be changed).
        /// </summary>
        /// <returns>
        /// A JWT Authorization token suitable to call Ganymede gRPC APIs.
        /// </returns>
        public static string GetToken()
        {
            var auth0Token = Environment.GetEnvironmentVariable("AUTH0_TOKEN");
            var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            var audience = Environment.GetEnvironmentVariable("AUDIENCE");
            var tenant = Environment.GetEnvironmentVariable("TENANT");

            // If we have AUTH0_TOKEN, generate a bearer token
            if (!string.IsNullOrEmpty(auth0Token))
            {
                return CreateBearerToken(auth0Token);
            }

            // If we don't, look for CLIENT_ID, CLIENT_SECRET, AUDIENCE and TENANT to create a token using Auth0 API
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                return CreateBearerTokenUsingRest(clientId, clientSecret, audience ?? DefaultAudience, tenant ?? DefaultTenant, out _);
            }

            throw new Exception($"{nameof(GetToken)}: AUTH0_TOKEN environment variable is not set, therefore CLIENT_ID and CLIENT_SECRET (and optionally AUDIENCE and TENANT) environment variables must be set");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create bearer token.
        /// </summary>
        /// <param name="auth0Token">
        /// The auth0 token.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string CreateBearerToken(string auth0Token) => $"Bearer {auth0Token}";

        /// <summary>
        /// Create bearer token using a POST request to https://{tenant}/oauth/token/.
        /// </summary>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="clientSecret">
        /// The client secret.
        /// </param>
        /// <param name="audience">
        /// The audience.
        /// </param>
        /// <param name="tenant">
        /// The tenant.
        /// </param>
        /// <param name="scope">
        /// The scope.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string CreateBearerTokenUsingRest(string clientId, string clientSecret, string audience, string tenant, out string scope)
        {
            var jsonContent = $@"{{ 
  ""client_id"": ""{clientId}"", 
  ""client_secret"": ""{clientSecret}"", 
  ""grant_type"" : ""client_credentials"", 
  ""audience"":  ""{audience}""
}}";

            string jsonResponse = null;
            var endPoint = new Uri($"https://{tenant}/oauth/token");
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, endPoint))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                                        $"{nameof(CreateBearerTokenUsingRest)} POST {endPoint} failed: StatusCode={response.StatusCode} ReasonPhrase={response.ReasonPhrase}");
                }

                using (var streamReader = new StreamReader(response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()))
                {
                    jsonResponse = streamReader.ReadToEnd();
                    using (var jsonDocument = JsonDocument.Parse(jsonResponse))
                    {
                        var tokenType = jsonDocument.RootElement.GetProperty("token_type").GetString();
                        var accessToken = jsonDocument.RootElement.GetProperty("access_token").GetString();
                        scope = jsonDocument.RootElement.GetProperty("scope").GetString();
                        if (!string.IsNullOrEmpty(tokenType) && !string.IsNullOrEmpty(accessToken))
                        {
                            return $"{tokenType} {accessToken}";
                        }
                    }
                }
            }

            throw new Exception(
                                $"{nameof(CreateBearerTokenUsingRest)}: Returned JSON doesn't contain 'token_type' and/or 'access_token'. Check your client ID, client secret, audience and tenant: {jsonResponse}");
        }

        #endregion
    }
}