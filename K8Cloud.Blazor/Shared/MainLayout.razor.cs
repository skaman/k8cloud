using MudBlazor;

namespace K8Cloud.Blazor.Shared
{
    public partial class MainLayout
    {
        private bool _drawerOpen = true;

        private bool _isDarkMode = true; // TODO: handle the default without UI blinking
        private MudThemeProvider _mudThemeProvider = null!;

        private readonly MudTheme _theme = new MudTheme
        {
            Palette = new PaletteLight
            {
                //Black = "#1e2428",
                Primary = "#005cc5",
                Secondary = "#6f42c1",
                Info = "#005cc5",
                Success = "#005cc5",
                Warning = "#e36209",
                Error = "#d73a49",
                //ActionDefault = "#959da5",
                Background = "#F7F8FA",
                BackgroundGrey = "#fafbfc",
                Surface = "#fafbfc",
                DrawerBackground = "#F3f3f3",
                AppbarBackground = "#2f363d"
            },
            PaletteDark = new PaletteDark
            {
                //Black = "#1e2428",
                Primary = "#7986CB",
                Secondary = "#b392f0",
                Info = "#79b8ff",
                Success = "#43A047",
                Warning = "#ffab70",
                Error = "#EF5350",
                //ActionDefault = "#959da5",
                Background = "#24292e",
                BackgroundGrey = "#2f363d",
                Surface = "#2f363d",
                DrawerBackground = "#1e2428",
                AppbarBackground = "#1e2428"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "0px",
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "300px"
            },
            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "Poppins", "Helvetica", "Arial", "sans-serif" }
                }
            }
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _isDarkMode = await _mudThemeProvider.GetSystemPreference();
                await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChanged);
                StateHasChanged();
            }
        }

        private Task OnSystemPreferenceChanged(bool newValue)
        {
            _isDarkMode = newValue;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }
    }
}
