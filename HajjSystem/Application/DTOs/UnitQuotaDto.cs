namespace HajjSystem.Application.DTOs;

public record UnitQuotaDto(
    int    UnitId,
    string UnitNameAr,
    int    AllowNumber,
    int    StandBy,
    int    RegularUsed,
    int    StandByUsed,
    int    RegularRemain,
    int    StandByRemain,
    int    RegularPct,
    int    StandByPct,
    bool   RegularFull,
    bool   StandByFull
);
