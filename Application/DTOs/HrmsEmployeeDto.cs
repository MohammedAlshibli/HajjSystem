namespace HajjSystem.Application.DTOs;

/// <summary>Raw DTO from HRMS API response.</summary>
public class HrmsEmployeeDto
{
    public string  NAME_ARABIC      { get; set; } = string.Empty;
    public string  SERVICE_NUMBER   { get; set; } = string.Empty;
    public string  EMP_STAT         { get; set; } = "0";
    public string  NIC_NO           { get; set; } = string.Empty;
    public DateTime? NIC_EXP_DATE   { get; set; }
    public string  PP_NO            { get; set; } = string.Empty;
    public DateTime? PP_EXP_DATE    { get; set; }
    public DateTime? DOB_T          { get; set; }
    public string  RANK_CODE        { get; set; } = "0";
    public string  RANK_ARABIC      { get; set; } = string.Empty;
    public string  UNIT             { get; set; } = "0";
    public string  SERVICE          { get; set; } = "0";
    public string  uniT_ARABIC      { get; set; } = string.Empty;
    public string  REGION_A         { get; set; } = string.Empty;
    public string  WIL_CODE         { get; set; } = "-";
    public string  WIL_ARABIC       { get; set; } = string.Empty;
    public string  VIL_CODE         { get; set; } = "-";
    public string  VIL_ARABIC       { get; set; } = string.Empty;
    public string  GSM              { get; set; } = string.Empty;
    public string  Blood_A          { get; set; } = string.Empty;
    public DateTime? DOE_T          { get; set; }
}
