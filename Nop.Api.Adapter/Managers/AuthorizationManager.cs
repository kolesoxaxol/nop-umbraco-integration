﻿using System;


namespace Nop.Api.Adapter.Managers
{
    public class AuthorizationManager
    {
        private readonly ApiAuthorizer _apiAuthorizer;

        public AuthorizationManager() {
            _apiAuthorizer = new ApiAuthorizer(AccessProvider.ClientId, AccessProvider.ClientSecret, AccessProvider.ServerUrl);
        }

        public string BuildAuthUrl(string redirectUrl, string[] requestedPermissions, string state = null)
        {
            var returnUrl = new Uri(redirectUrl);

            // get the Authorization URL and redirect the user
            var authUrl = _apiAuthorizer.GetAuthorizationUrl(returnUrl.ToString(), requestedPermissions, state);

            return authUrl;
        }

        public string GetAuthorizationData(AuthParameters authParameters)
        {
            if (!String.IsNullOrEmpty(authParameters.Error))
            {
                throw new Exception(authParameters.Error);
            }

            // make sure we have the necessary parameters
            ValidateParameter("code", authParameters.Code);
            ValidateParameter("storeUrl", authParameters.ServerUrl);
            ValidateParameter("clientId", authParameters.ClientId);
            ValidateParameter("clientSecret", authParameters.ClientSecret);
            ValidateParameter("RedirectUrl", authParameters.RedirectUrl);
            ValidateParameter("GrantType", authParameters.GrantType);

            // get the access token
            var accessToken = _apiAuthorizer.AuthorizeClient(authParameters.Code, authParameters.GrantType, authParameters.RedirectUrl);

            return accessToken;
        }

        public string RefreshAuthorizationData(AuthParameters authParameters)
        {
            if (!String.IsNullOrEmpty(authParameters.Error))
            {
                throw new Exception(authParameters.Error);
            }

            // make sure we have the necessary parameters
            ValidateParameter("storeUrl", authParameters.ServerUrl);
            ValidateParameter("clientId", authParameters.ClientId);
            ValidateParameter("GrantType", authParameters.GrantType);
            ValidateParameter("RefreshToken", authParameters.RefreshToken);

            // get the access token
            var accessToken = _apiAuthorizer.RefreshToken(authParameters.RefreshToken, authParameters.GrantType);

            return accessToken;
        }

        private void ValidateParameter(string parameterName, string parameterValue)
        {
            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                throw new Exception($"{parameterName} parameter is missing");
            }
        }
    }
}