using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GraphUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public string LaunchPath { get; set; }
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            AppActivationArguments appActivationArguments = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

            if (appActivationArguments.Kind is ExtendedActivationKind.File && appActivationArguments.Data is IFileActivatedEventArgs fileActivatedEventArgs && fileActivatedEventArgs.Files.FirstOrDefault() is IStorageFile storageFile)
                LaunchPath = storageFile.Path;

            m_window = new MainWindow(LaunchPath);
            m_window.Activate();
           
        }

        private Window m_window;
    }
}
