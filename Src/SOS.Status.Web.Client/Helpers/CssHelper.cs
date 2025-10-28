using SOS.Status.Web.Client.Dtos;

namespace SOS.Status.Web.Client.Helpers;

public static class CssHelper
{
    public static Func<DataProviderStatusDto, string> PublicObservationCellClassFunc => m => GetStatusCellDiffClass(m.PublicActive, m.PublicInactive);
    public static Func<DataProviderStatusDto, string> ProtectedObservationCellClassFunc => m => GetStatusCellDiffClass(m.ProtectedActive, m.ProtectedInactive);
    public static Func<DataProviderStatusDto, string> InvalidObservationCellClassFunc => m => GetStatusCellDiffClass(m.InvalidInactive, m.InvalidActive);
    public static Func<DataProviderStatusDto, string> PublicObservationCellStyleFunc => m => GetStatusCellDiffStyle(m.PublicActive, m.PublicInactive);
    public static Func<DataProviderStatusDto, string> ProtectedObservationCellStyleFunc => m => GetStatusCellDiffStyle(m.ProtectedActive, m.ProtectedInactive);
    public static Func<DataProviderStatusDto, string> InvalidObservationCellStyleFunc => m => GetStatusCellDiffStyle(m.InvalidInactive, m.InvalidActive);

    public static string GetStatusCellDiffClass(int activeValue, int inactiveValue)
    {
        int diff = activeValue - inactiveValue;
        if (diff >= 0)
            return "bg-success";
        else if ((double)activeValue / inactiveValue >= 0.95)
            return "bg-warning";
        else
            return "bg-danger";
    }

    public static string GetStatusCellDiffStyle(int activeValue, int inactiveValue, ColorMode colorMode = ColorMode.Default)
    {
        int diff = activeValue - inactiveValue;
        string bgColor;

        if (diff >= 0)
        {
            if (colorMode == ColorMode.NoColorWhenPositiveDiff)
            {
                return "";
            }
            if (colorMode == ColorMode.NoColorWhenNoChange && diff == 0)
            {
                return "";
            }

            bgColor = ColorHelper.SuccessColor;
        }
        else if (inactiveValue != 0 && (double)activeValue / inactiveValue >= 0.95)
            bgColor = ColorHelper.WarningColor;
        else
            bgColor = ColorHelper.DangerColor;

        //if (diff >= 0)
        //    bgColor = CustomTheme.MyCustomTheme.PaletteLight.SuccessDarken;
        //else if (inactiveValue != 0 && (double)activeValue / inactiveValue >= 0.95)
        //    bgColor = CustomTheme.MyCustomTheme.PaletteLight.WarningLighten;
        //else
        //    bgColor = CustomTheme.MyCustomTheme.PaletteLight.Error.Value;

        return $"background-color: {bgColor};";
    }

    public enum ColorMode
    {
        Default,
        NoColorWhenPositiveDiff,
        NoColorWhenNoChange
    }
}