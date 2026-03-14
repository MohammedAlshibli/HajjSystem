namespace HajjSystem.Application.DTOs;

public record PilgrimDto(
    int      PilgrimId,
    string   FullName,
    string   ServiceNumber,
    string   NIC,
    string   RankDesc,
    string?  Region,
    string   ArabicName,
    int      TypeId,
    int      FitResult,
    int      ConfirmCode,
    string?  BloodGroup,
    string   Passport,
    string?  PassportExpire,
    string?  NICExpire,
    string?  CancelNote,
    string?  DoctorNote,
    string?  InjectionDate,
    int      HajjYear
);
