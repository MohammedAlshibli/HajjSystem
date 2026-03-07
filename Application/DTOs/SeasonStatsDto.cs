namespace HajjSystem.Application.DTOs;

public record SeasonStatsDto(
    int  Year,
    int  Total,
    int  HQApproved,
    int  Pending,
    int  Cancelled,
    int  Regular,
    int  StandBy,
    int  Admin,
    bool IsCurrentYear,
    IEnumerable<UnitBreakdownDto> Units
);

public record UnitBreakdownDto(
    int    UnitId,
    string UnitName,
    int    AllowNumber,
    int    StandBy,
    int    Registered,
    int    HQApproved
);
