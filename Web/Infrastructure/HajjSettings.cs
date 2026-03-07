namespace HajjSystem.Web.Infrastructure;

public class HajjSettings
{
    public int    ActiveHajjYear       { get; set; }
    public string HrmsBaseUrl          { get; set; } = string.Empty;
    public string HrmsEmployeeEndpoint { get; set; } = string.Empty;
    public string ServiceNumberPattern { get; set; } = @"^[A-Z0-9]{4,12}$";
}
