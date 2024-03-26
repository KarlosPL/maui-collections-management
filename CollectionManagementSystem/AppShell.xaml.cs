using CollectionManagementSystem.Pages;

namespace CollectionManagementSystem
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(NewCollectionPage), typeof(NewCollectionPage));
            Routing.RegisterRoute(nameof(SingleCollectionPage), typeof(SingleCollectionPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }
    }
}
