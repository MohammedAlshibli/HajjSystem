namespace HajjSystem.Domain.Constants;

public static class HajjConstants
{
    public static class PilgrimType
    {
        public const int Regular = 1;
        public const int StandBy = 2;
        public const int Admin   = 3;
    }

    public static class FitResult
    {
        public const int Pending          = 7;
        public const int Fit              = 5;
        public const int ConditionallyFit = 8;
        public const int NotFit           = 6;
        public const int DoctorApproved   = 9;
    }

    public static class ConfirmCode
    {
        public const int Pending    = 0;
        public const int Confirmed  = 51;
        public const int Cancelled  = 99;
        public const int HQApproved = 77;
    }

    public static class FlightDirection
    {
        public const int Departure = 34;
        public const int Return    = 35;
    }

    public static class ParamCode
    {
        public const string ClassType   = "ClassType";
        public const string FitCode     = "FitCode";
        public const string ConfirmCode = "ConfirmCode";
    }
}
