using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Palettes;

namespace ActivityDesk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Set the color palette used by the application
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            SurfaceColors.SetDefaultApplicationPalette(new DarkSurfacePalette());
        }  
    }
}