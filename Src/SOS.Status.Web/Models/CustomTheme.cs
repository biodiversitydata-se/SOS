using MudBlazor;

namespace SOS.Status.Web.Models;

public static class CustomTheme
{
    public static MudTheme MyCustomTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = Colors.Blue.Default,
            Secondary = Colors.Green.Accent4,
            Background = "#FFFFFF",
            //Background = "#F5F5F5",
            //AppbarBackground = Colors.Blue.Darken1,
            AppbarBackground = Colors.Green.Default,
            DrawerBackground = "#FFFFFF",
            TextPrimary = "#212121",
            
            //Success = "#1e7e34"
        }
    };
}