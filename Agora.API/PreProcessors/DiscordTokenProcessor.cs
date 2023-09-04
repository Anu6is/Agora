//using Agora.API.Services;
//using FastEndpoints;
//using FluentValidation.Results;
//using Microsoft.AspNetCore.Http;

//namespace Agora.API.PreProcessors
//{
//    public class Member
//    {
//        public string Id { get; set; }
//        public string Code { get; set; }
//    }

//    public class DiscordTokenProcessor : IPreProcessor<Member>
//    {
//        public async Task PreProcessAsync(Member req, HttpContext ctx, List<ValidationFailure> failures, CancellationToken ct)
//        {
//            var tokenService = ctx.Resolve<AccessTokenService>();

//            var user = await tokenService.GetUserInfoAsync(req.Code);

//            throw new NotImplementedException();
//        }
//    }
//}
