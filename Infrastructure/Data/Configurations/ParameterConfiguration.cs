using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using HajjSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> b)
    {
        b.HasKey(p => p.Id);

        b.Property(p => p.Type)
            .HasConversion<string>()   // stores "ClassType", "FitCode" etc. — readable in DB
            .HasMaxLength(30)
            .IsRequired();

        b.Property(p => p.DescArabic).HasMaxLength(100);
        b.Property(p => p.DescEnglish).HasMaxLength(100);

        // Unique: same type cannot have duplicate values
        b.HasIndex(p => new { p.Type, p.Value }).IsUnique();

        // ── Seed ─────────────────────────────────────────────────────
        b.HasData(

            // ClassType (discriminator = ParameterType.ClassType)
            Row(1,  ParameterType.ClassType,       HajjConstants.PilgrimType.Regular,          "أصيل",            "Regular"),
            Row(2,  ParameterType.ClassType,       HajjConstants.PilgrimType.StandBy,          "احتياطي",          "StandBy"),
            Row(3,  ParameterType.ClassType,       HajjConstants.PilgrimType.Admin,            "إداري",            "Admin"),

            // FitCode
            Row(4,  ParameterType.FitCode,         HajjConstants.FitResult.Pending,            "معلق",             "Pending"),
            Row(5,  ParameterType.FitCode,         HajjConstants.FitResult.Fit,                "مؤهل",             "Fit"),
            Row(6,  ParameterType.FitCode,         HajjConstants.FitResult.ConditionallyFit,   "مؤهل مشروط",       "Conditionally Fit"),
            Row(7,  ParameterType.FitCode,         HajjConstants.FitResult.NotFit,             "غير مؤهل",         "Not Fit"),
            Row(8,  ParameterType.FitCode,         HajjConstants.FitResult.DoctorApproved,     "مؤهل من الطبيب",   "Doctor Approved"),

            // ConfirmCode
            Row(9,  ParameterType.ConfirmCode,     HajjConstants.ConfirmCode.Pending,          "معلق",             "Pending"),
            Row(10, ParameterType.ConfirmCode,     HajjConstants.ConfirmCode.Confirmed,        "مؤكد",             "Confirmed"),
            Row(11, ParameterType.ConfirmCode,     HajjConstants.ConfirmCode.Cancelled,        "ملغي",             "Cancelled"),
            Row(12, ParameterType.ConfirmCode,     HajjConstants.ConfirmCode.HQApproved,       "معتمد مركزي",      "HQ Approved"),

            // FlightDirection
            Row(13, ParameterType.FlightDirection, HajjConstants.FlightDirection.Departure,    "ذهاب",             "Departure"),
            Row(14, ParameterType.FlightDirection, HajjConstants.FlightDirection.Return,       "عودة",             "Return")
        );
    }

    private static Parameter Row(int id, ParameterType type, int value, string ar, string en) => new()
    {
        Id          = id,
        Type        = type,
        Value       = value,
        DescArabic  = ar,
        DescEnglish = en,
        IsActive    = true,
        TenantId    = 0,
        IsDeleted   = false,
        CreatedBy   = "SYSTEM",
        CreatedOn   = new DateTime(2024, 1, 1)
    };
}
