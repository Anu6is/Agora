﻿using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Domain.Common;
using Emporia.Domain.Services;

namespace Agora.Shared.Features.Commands
{
    public class CreateUserProfileCommand : Command<IResult<UserProfile>>
    {
        public UserId UserId { get; init; }
        public EmporiumId EmporiumId { get; init; }
        public ReferenceNumber UserReference { get; init; }
        public bool OutbidAlerts { get; set; }

        public CreateUserProfileCommand(EmporiumId emporiumId, UserId userId, ReferenceNumber userReference)
        {
            UserId = userId;
            EmporiumId = emporiumId;
            UserReference = userReference;
        }
    }
}
