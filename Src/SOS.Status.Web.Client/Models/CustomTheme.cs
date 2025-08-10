using MudBlazor;

namespace SOS.Status.Web.Client.Models;

public static class CustomTheme
{
    public static MudTheme MyCustomTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {            
            Primary = "#007681",
            //Primary = "#004851",
            //PrimaryContrastText = "#FFFFFF",
            //PrimaryDarken = "#005a5c",
            //PrimaryLighten = "#339899",            
            Secondary = Colors.Green.Accent4,
            Background = "#FFFFFF",
            //Background = "#F5F5F5",
            //AppbarBackground = Colors.Blue.Darken1,
            ///AppbarBackground = Colors.Green.Default,
            AppbarBackground = "#017681",
            DrawerBackground = "#FFFFFF",
            TextPrimary = "#212121",
            //Success = "#1e7e34"
        }
    };
}