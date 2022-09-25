using Agora.Shared.Persistence.Models;
using Emporia.Domain.Entities;
using Emporia.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agora.Shared.Persistence
{
    public class UserProfileConfiguration : EntityTypeConfiguration<UserProfile>
    {
        public override void Configure(EntityTypeBuilder<UserProfile> builder, DatabaseFacade database)
        {
            builder.ToTable("UserProfile");
            builder.HasKey(profile => profile.Id);
            builder.Property(profile => profile.OutbidAlerts);
            builder.Property(profile => profile.EmporiumId);
            builder.HasIndex(profile => profile.UserReference);
            builder.HasOne<Emporium>().WithMany().HasForeignKey(x => x.EmporiumId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
