using HajjSystem.Domain.Constants;
using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> b)
    {
        b.HasKey(p => p.ParameterId);

        b.Property(p => p.Code).IsRequired().HasMaxLength(50);
        b.Property(p => p.DescArabic).HasMaxLength(100);
        b.Property(p => p.DescEnglish).HasMaxLength(100);

        // ── Seed required lookup data ─────────────────────────────────
        b.HasData(

            // Pilgrim Type
            Seed(HajjConstants.PilgrimType.Regular, HajjConstants.ParamCode.ClassType, "أصيل",           "Regular"),
            Seed(HajjConstants.PilgrimType.StandBy, HajjConstants.ParamCode.ClassType, "احتياطي",         "StandBy"),
            Seed(HajjConstants.PilgrimType.Admin,   HajjConstants.ParamCode.ClassType, "إداري",           "Admin"),

            // Fit Result
            Seed(HajjConstants.FitResult.Pending,          HajjConstants.ParamCode.FitCode, "معلق",            "Pending"),
            Seed(HajjConstants.FitResult.Fit,              HajjConstants.ParamCode.FitCode, "مؤهل",            "Fit"),
            Seed(HajjConstants.FitResult.ConditionallyFit, HajjConstants.ParamCode.FitCode, "مؤهل مشروط",      "Conditionally Fit"),
            Seed(HajjConstants.FitResult.NotFit,           HajjConstants.ParamCode.FitCode, "غير مؤهل",        "Not Fit"),
            Seed(HajjConstants.FitResult.DoctorApproved,   HajjConstants.ParamCode.FitCode, "مؤهل من الطبيب",  "Doctor Approved"),

            // Flight Direction
            Seed(HajjConstants.FlightDirection.Departure, "FlightDirection", "ذهاب", "Departure"),
            Seed(HajjConstants.FlightDirection.Return,    "FlightDirection", "عودة", "Return"),

            // Confirm Code
            Seed(HajjConstants.ConfirmCode.Pending,    HajjConstants.ParamCode.ConfirmCode, "معلق",          "Pending"),
            Seed(HajjConstants.ConfirmCode.Confirmed,  HajjConstants.ParamCode.ConfirmCode, "مؤكد",          "Confirmed"),
            Seed(HajjConstants.ConfirmCode.Cancelled,  HajjConstants.ParamCode.ConfirmCode, "ملغي",          "Cancelled"),
            Seed(HajjConstants.ConfirmCode.HQApproved, HajjConstants.ParamCode.ConfirmCode, "معتمد مركزي",   "HQ Approved")
        );
    }

    /// <summary>Helper — creates a Parameter seed row with standard audit defaults.</summary>
    private static Parameter Seed(int id, string code, string ar, string en) => new()
    {
        ParameterId  = id,
        Code         = code,
        DescArabic   = ar,
        DescEnglish  = en,
        Value        = id,
        // BaseEntity audit fields must have fixed values for seeding
        TenantId     = 0,
        IsDeleted    = false,
        CreatedBy    = "SYSTEM",
        CreatedOn    = new DateTime(2024, 1, 1)
    };
}
