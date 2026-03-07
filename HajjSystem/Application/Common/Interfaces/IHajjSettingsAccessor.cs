namespace HajjSystem.Application.Common.Interfaces;

public interface IHajjSettingsAccessor
{
    int  ActiveHajjYear { get; }
    void UpdateActiveYear(int newYear);
}
