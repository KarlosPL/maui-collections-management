using System.Diagnostics;

namespace CollectionManagementSystem
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            Debug.WriteLine($"Ścieżka do plików zapisanych: {FileSystem.AppDataDirectory}");
        }
    }
}
