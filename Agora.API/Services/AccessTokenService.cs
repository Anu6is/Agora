//using Auth0.AuthenticationApi;
//using Auth0.AuthenticationApi.Models;
//using Microsoft.Extensions.Configuration;

//namespace Agora.API.Services
//{
//    public class AccessTokenService
//    {
//        private readonly string _clientId;
//        private readonly string _clientSecret;
//        private readonly string _redirectUrl;
//        private readonly AuthenticationApiClient _authClient;

//        public AccessTokenService(IConfiguration configuration)
//        {
//            _authClient = new AuthenticationApiClient(configuration["Auth0:Domain"]);
//            _clientId = configuration["Auth0:ClientId"]!;
//            _clientSecret = configuration["Auth0:ClientSecret"]!;
//            _redirectUrl = configuration["Auth0:RedirectUri"]!;
//        }

//        public async Task<string> GenerateAccessTokenAsync(string code)
//        {
//            var token = await _authClient.GetTokenAsync(new AuthorizationCodeTokenRequest
//            {
//                ClientId = _clientId,
//                ClientSecret = _clientSecret,
//                RedirectUri = _redirectUrl,
//                Code = code
//            });
            
//            return token.AccessToken;
//        }

//        public async Task<UserInfo> GetUserInfoAsync(string code)
//        {
//            var token = await GenerateAccessTokenAsync(code);
//            var user = await _authClient.GetUserInfoAsync(token);
            
//            return user;
//        }
//    }
//}