using MudBlazor;

namespace SpinnerNet.App.Themes;

public static class NatureTheme
{
    public static MudTheme GardenTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#34d399", // Soft green - growing plants
            Secondary = "#f472b6", // Soft pink - flowers
            Tertiary = "#fbbf24", // Sunflower yellow - sunshine
            Success = "#10b981", // Deep green - healthy growth
            Info = "#60a5fa", // Sky blue - clear skies
            Warning = "#f59e0b", // Amber - autumn leaves
            Error = "#ef4444", // Soft red - roses
            Dark = "#1f2937", // Rich soil
            Background = "#fefce8", // Warm cream - morning light
            Surface = "#ffffff", // Pure white - clouds
            DrawerBackground = "#ffffff",
            DrawerText = "#1f2937",
            DrawerIcon = "#6b7280",
            AppbarBackground = "#34d399",
            AppbarText = "#ffffff",
            TextPrimary = "#1f2937",
            TextSecondary = "#6b7280",
            ActionDefault = "#9ca3af",
            ActionDisabled = "#d1d5db",
            ActionDisabledBackground = "#f3f4f6",
            Divider = "#e5e7eb",
            DividerLight = "#f3f4f6",
            TableLines = "#e5e7eb",
            LinesDefault = "#e5e7eb",
            LinesInputs = "#d1d5db",
            TextDisabled = "#9ca3af"
        }
    };
}