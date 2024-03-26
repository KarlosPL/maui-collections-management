using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Pages;

public partial class NewCollectionPage : ContentPage
{
	public NewCollectionPage()
	{
		InitializeComponent();
		BindingContext = CollectionsManagementVM.Instance;
	}
}