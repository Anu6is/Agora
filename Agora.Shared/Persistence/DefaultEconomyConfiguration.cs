using Agora.Shared.Persistence.Models;
using Emporia.Domain.Entities;
using Emporia.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agora.Shared.Persistence
{
    public class DefaultEconomyConfiguration : EntityTypeConfiguration<DefaultEconomyUser>
    {
        public override void Configure(EntityTypeBuilder<DefaultEconomyUser> builder, DatabaseFacade database)
        {
            builder.ToTable("Economy");
            builder.HasKey(economy => economy.UserId);
            builder.Property(economy => economy.Balance);
            builder.Property(economy => economy.EmporiumId);
            builder.HasIndex(economy => economy.UserReference);
            builder.HasOne<Emporium>().WithMany().HasForeignKey(x => x.EmporiumId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
