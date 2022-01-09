using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SvadbeniSalon.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Configurations
{
    public static class DefaultDate
    {
        public static PropertyBuilder<DateTime> HasDefaultDate(this EntityTypeBuilder<BaseEntity> entityTypeBuilder)
        {
            return entityTypeBuilder.Property(p => p.CreatedAt).HasDefaultValue(DateTime.UtcNow);
        }
    }
}
