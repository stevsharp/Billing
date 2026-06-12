using Billing.Domain.Invoices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("Invoices");

        b.HasKey(i => i.Id);
        b.Property(i => i.Id)
            .HasConversion(id => id.Value, v => new InvoiceId(v))
            .ValueGeneratedNever();

        b.Property(i => i.Number)
            .HasConversion(n => n.Value, v => new InvoiceNumber(v))
            .HasMaxLength(40);
        b.HasIndex(i => i.Number).IsUnique().HasFilter("[IsDeleted] = 0");

        b.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        b.OwnsOne(i => i.Total, m =>
        {
            m.Property(x => x.Amount).HasColumnName("Total_Amount").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("Total_Currency").HasMaxLength(3);
        });
        b.Navigation(i => i.Total).IsRequired();

        b.OwnsMany(i => i.Lines, l =>
        {
            l.ToTable("InvoiceLines");
            l.WithOwner().HasForeignKey("InvoiceId");
            l.HasKey(x => x.Id);
            l.Property(x => x.Description).HasMaxLength(200);
            l.OwnsOne(x => x.Amount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            l.Navigation(x => x.Amount).IsRequired();
        });
        b.Navigation(i => i.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Cross-cutting concerns as shadow properties (invisible to the domain)
        b.Property<Guid>("TenantId");
        b.Property<DateTime>("CreatedOn");
        b.Property<string?>("CreatedBy");
        b.Property<DateTime?>("ModifiedOn");
        b.Property<string?>("ModifiedBy");
        b.Property<bool>("IsDeleted");
        b.Property<DateTime?>("DeletedOn");
        b.Property<string?>("DeletedBy");

        b.Ignore(i => i.DomainEvents);
    }
}
