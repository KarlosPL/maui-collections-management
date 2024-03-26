using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = CollectionsManagementVM.Instance;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CollectionsManagementVM.Instance.ReadCollectionsFromFile();
        }

    }

}
