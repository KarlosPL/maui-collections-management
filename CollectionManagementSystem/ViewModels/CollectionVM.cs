using CollectionManagementSystem.Models;
using CollectionManagementSystem.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CollectionManagementSystem.ViewModels
{
    [QueryProperty(nameof(CollectionId), nameof(CollectionId))]
    [QueryProperty(nameof(CollectionName), nameof(CollectionName))]
    public partial class CollectionVM : ObservableObject
    {
        private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "items.txt");
        private static CollectionVM? _instance;
        public static CollectionVM Instance => _instance ??= new CollectionVM();

        [ObservableProperty]
        string? collectionId;

        [ObservableProperty]
        string? collectionName;

        [ObservableProperty]
        public ObservableCollection<ItemCollection> items;

        [ObservableProperty]
        public string? name;

        [ObservableProperty]
        public string? price;

        [ObservableProperty]
        public List<string> statuses;

        [ObservableProperty]
        public string? status;

        [ObservableProperty]
        public string? rating;

        [ObservableProperty]
        public string? comment;

        [ObservableProperty]
        public string? imagePath;

        [ObservableProperty]
        public string? customName;

        [ObservableProperty]
        public string? customValue;

        public CollectionVM()
        {
            Items = [];
            Statuses = ["nowy", "użyty", "na sprzedaż", "sprzedany", "chcę kupić"];
        }

        [RelayCommand]
        public async Task RenameCollection()
        {
            var newName = await Application.Current.MainPage.DisplayPromptAsync("Zmiana nazwy", "Wprowadź nową nazwę kolekcji:");
            if (!string.IsNullOrEmpty(newName))
            {
                await CollectionsManagementVM.Instance.RenameCollectionById(CollectionId, newName);
                CollectionName = newName;
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        public async Task DeleteCollection()
        {
            await CollectionsManagementVM.Instance.DeleteCollectionById(CollectionId);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task GoToNewItemPage()
        {
            Name = string.Empty;
            Price = string.Empty;
            Status = string.Empty;
            Rating = string.Empty;
            Comment = string.Empty;
            ImagePath = string.Empty;
            CustomName = string.Empty;
            CustomValue = string.Empty;
            SelectedItem = null;
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        [ObservableProperty]
        ItemCollection? selectedItem;

        public string SaveOrUpdateButtonText => SelectedItem == null ? "Dodaj element" : "Zapisz zmiany";

        [RelayCommand]
        public async Task EditItem(ItemCollection item)
        {
            Name = string.Empty;
            Price = string.Empty;
            Status = string.Empty;
            Rating = string.Empty;
            Comment = string.Empty;
            ImagePath = string.Empty;
            CustomName = string.Empty;
            CustomValue = string.Empty;
            SelectedItem = item;
            await Shell.Current.GoToAsync($"{nameof(NewItemPage)}", true, new Dictionary<string, object>
            {
                { "Item", item }
            });
        }

        [RelayCommand]
        public async Task RemoveItem(ItemCollection itemToRemove)
        {
            if (itemToRemove == null) return;

            Items.Remove(itemToRemove);

            await WriteItemsToFile();
        }


        [RelayCommand]
        public async Task AddOrUpdateItem()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Nazwa jest wymagana", "OK");
                return;
            }

            int priceParsed = int.TryParse(Price, out priceParsed) ? priceParsed : 0;
            int ratingParsed = int.TryParse(Rating, out ratingParsed) ? ratingParsed : 0;

            if (SelectedItem == null && Items.Any(item => item.Name == Name))
            {
                bool continueAdding = await Application.Current.MainPage.DisplayAlert("Uwaga", "Element o takiej nazwie już istnieje. Czy na pewno chcesz dodać duplikat?", "Tak", "Nie");
                if (!continueAdding) return;
            }

            if (SelectedItem == null)
            {
                var newItem = new ItemCollection
                {
                    Id = NanoidDotNet.Nanoid.Generate(size: 10),
                    AssignedCollection = CollectionId,
                    Name = Name,
                    Price = priceParsed,
                    Status = Status ?? "empty",
                    Rating = ratingParsed,
                    Comment = Comment ?? "empty",
                    ImagePath = ImagePath ?? "empty",
                    CustomName = CustomName ?? "empty",
                    CustomValue = CustomValue ?? "empty"
                };

                Items.Add(newItem);
            }
            else
            {
                SelectedItem.Name = Name;
                SelectedItem.Price = priceParsed;
                SelectedItem.Status = Status ?? "empty";
                SelectedItem.Rating = ratingParsed;
                SelectedItem.Comment = Comment ?? "empty";
                SelectedItem.CustomName = CustomName ?? "empty";
                SelectedItem.CustomValue = CustomValue ?? "empty";

                var index = Items.IndexOf(SelectedItem);
                if (index != -1)
                {
                    Items[index] = SelectedItem;
                }
            }

            await WriteItemsToFile();

            Name = string.Empty;
            Price = string.Empty;
            Status = string.Empty;
            Rating = string.Empty;
            Comment = string.Empty;
            ImagePath = string.Empty;
            CustomName = string.Empty;
            CustomValue = string.Empty;
            SelectedItem = null;

            await Shell.Current.GoToAsync("..");
        }

        public void SortItems()
        {
            var sortedItems = Items.OrderBy(item => item.Status == "sprzedany").ToList();
            Items = new ObservableCollection<ItemCollection>(sortedItems);
        }

        [RelayCommand]
        public async Task PickImage()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Wybierz obraz",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                ImagePath = result.FullPath;
                if (SelectedItem != null)
                {
                    SelectedItem.ImagePath = result.FullPath;
                }
            }
            else
            {
                ImagePath = null;
            }
        }

        [RelayCommand]
        public async Task Wrapped()
        {
            
            await Application.Current.MainPage.DisplayAlert($"Podsumowanie kolekcji {CollectionName}", 
                $"Ilość przedmiotów: {Items.Count} \n" +
                $"Ilość przedmiotów sprzedanych: {Items.Count(item => item.Status == "sprzedany")} \n" +
                $"Ilość przedmiotów do sprzedaży: {Items.Count(item => item.Status == "na sprzedaż")}",
                "OK");
        }

        [RelayCommand]
        public async Task ExportCollection()
        {
            try
            {
                var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var fileName = $"items_{CollectionId}_{DateTime.Now:yyyyMMddHHmm}.txt";
                var savePath = Path.Combine(downloadsPath, fileName);

                var lines = Items.Select(item => $"{CollectionId},{item.Id},{item.Name},{item.Price},{item.Status},{item.Rating},{item.Comment},{item.ImagePath},{item.CustomName},{item.CustomValue}");

                await File.WriteAllLinesAsync(savePath, lines);

                await Application.Current.MainPage.DisplayAlert("Eksport zakończony", $"Kolekcja została zapisana w folderze Pobrane: {fileName}", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd eksportu kolekcji: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się wyeksportować kolekcji.", "OK");
            }
        }


        private async Task WriteItemsToFile()
        {
            var existingLines = File.Exists(filePath) ? await File.ReadAllLinesAsync(filePath) : [];
            var updatedLines = existingLines.Where(line => !line.StartsWith($"{CollectionId},")).ToList();

            updatedLines.AddRange(Items.Select(item => $"{CollectionId},{item.Id},{item.Name},{item.Price},{item.Status},{item.Rating},{item.Comment},{item.ImagePath},{item.CustomName},{item.CustomValue}"));

            await File.WriteAllLinesAsync(filePath, updatedLines);
        }

        public async Task ReadItemsFromFile()
        {
            Items.Clear();

            if (File.Exists(filePath))
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 10 && parts[0] == CollectionId)
                    {
                        Items.Add(new ItemCollection
                        {
                            Id = parts[1],
                            AssignedCollection = parts[0],
                            Name = parts[2],
                            Price = int.Parse(parts[3]),
                            Status = parts[4],
                            Rating = int.Parse(parts[5]),
                            Comment = parts[6],
                            ImagePath = parts[7],
                            CustomName = parts[8],
                            CustomValue = parts[9]
                        });
                    }
                }
            }

            SortItems();
        }

    }
}
