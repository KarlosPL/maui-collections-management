using CollectionManagementSystem.Models;
using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Pages;

[QueryProperty(nameof(Item), "Item")]
public partial class NewItemPage : ContentPage
{
    public ItemCollection Item
    {
        set
        {
            BindingContext = CollectionVM.Instance;
            CollectionVM.Instance.Name = value.Name;
            CollectionVM.Instance.Price = value.Price.ToString();
            CollectionVM.Instance.Status = value.Status;
            CollectionVM.Instance.Rating = value.Rating.ToString();
            CollectionVM.Instance.Comment = value.Comment;
            CollectionVM.Instance.ImagePath = value.ImagePath;
            CollectionVM.Instance.CustomName = value.CustomName;
            CollectionVM.Instance.CustomValue = value.CustomValue;
            CollectionVM.Instance.SelectedItem = value;
        }
    }

    public NewItemPage()
	{
		InitializeComponent();
		BindingContext = CollectionVM.Instance;
	}
}