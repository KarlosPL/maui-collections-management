using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Pages;

public partial class SingleCollectionPage : ContentPage
{
	public SingleCollectionPage()
	{
		InitializeComponent();
		BindingContext = CollectionVM.Instance;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CollectionVM viewModel)
        {
            await viewModel.ReadItemsFromFile();
        }
    }

}