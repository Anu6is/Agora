using Agora.Shared.Features.Commands;
using Agora.Shared.Persistence.Models;
using Emporia.Domain.Events;
using Emporia.Extensions.Discord;
using MediatR;

namespace Agora.Shared.Events
{
    internal class ReivewAdded : INotificationHandler<ReviewAddedNotification>
    {
        private readonly IMediator _mediator;
        private readonly IUserProfileService _userProfileService;

        public ReivewAdded(IUserProfileService userProfileService, IMediator mediator)
        {
            _mediator = mediator;
            _userProfileService = userProfileService;
        }

        public async Task Handle(ReviewAddedNotification notification, CancellationToken cancellationToken)
        {
            var profile = await _userProfileService.GetUserProfileAsync(notification.EmporiumUser.EmporiumId.Value,
                                                                        notification.Review.ReferenceNumber.Value) as UserProfile;

            var score = notification.EmporiumUser.Reviews.GroupBy(x => x.Rating, (key, value) => key * value.Count()).Sum();
            var reviews = notification.EmporiumUser.Reviews.Count;
            var rating = Math.Round((decimal)score / reviews, 1);

            profile.SetReviewCount((ulong)reviews).SetRating(rating);

            await _mediator.Send(new UpdateUserProfileCommand(profile), cancellationToken);

            return;
        }
    }
}
