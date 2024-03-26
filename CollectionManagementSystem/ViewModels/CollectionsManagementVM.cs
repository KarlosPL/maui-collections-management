using CollectionManagementSystem.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CollectionManagementSystem.Pages;
using System.Diagnostics;

namespace CollectionManagementSystem.ViewModels
{
    public partial class CollectionsManagementVM : ObservableObject
    {
        private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "collections.txt");
        private static CollectionsManagementVM? _instance;
        public static CollectionsManagementVM Instance => _instance ??= new CollectionsManagementVM();

        [ObservableProperty]
        private ObservableCollection<SingleCollection> collections;

        [ObservableProperty]
        public string? enteredName;

        public CollectionsManagementVM()
        {
            Collections = [];
        }

        [RelayCommand]
        public async Task GoToNewCollectionPage()
        {
            await Shell.Current.GoToAsync($"{nameof(NewCollectionPage)}");
        }

        [RelayCommand]
        public async Task AddNewCollection()
        {
            if (!string.IsNullOrWhiteSpace(EnteredName))
            {
                var newCollection = new SingleCollection { 
                    Id = NanoidDotNet.Nanoid.Generate(size: 8),
                    Name = EnteredName, 
                };

                Collections.Add(newCollection);

                await WriteCollectionsToFile();

                EnteredName = string.Empty;

                await Shell.Current.GoToAsync("..");
            }
        }

        [ObservableProperty]
        private SingleCollection? selectedCollection;

        [RelayCommand]
        public async Task Tap()
        {
            if (SelectedCollection != null)
            {
                await Shell.Current.GoToAsync($"{nameof(SingleCollectionPage)}?CollectionId={SelectedCollection.Id}&CollectionName={SelectedCollection.Name}");
                SelectedCollection = null;
            }
        }

        public async Task RenameCollectionById(string collectionId, string newName)
        {
            var collectionToRename = Collections.FirstOrDefault(c => c.Id == collectionId);
            if (collectionToRename != null)
            {
                collectionToRename.Name = newName;

                await WriteCollectionsToFile();
            }
        }

        public async Task DeleteCollectionById(string collectionId)
        {
            var collectionToDelete = Collections.FirstOrDefault(c => c.Id == collectionId);
            if (collectionToDelete != null)
            {
                Collections.Remove(collectionToDelete);
                await WriteCollectionsToFile();
                await DeleteItemsByCollectionId(collectionId);
            }
        }

        private static async Task DeleteItemsByCollectionId(string collectionId)
        {
            var itemsFilePath = Path.Combine(FileSystem.AppDataDirectory, "items.txt");
            if (File.Exists(itemsFilePath))
            {
                var allItems = await File.ReadAllLinesAsync(itemsFilePath);
                var itemsToKeep = allItems.Where(line => !line.StartsWith($"{collectionId},"));

                await File.WriteAllLinesAsync(itemsFilePath, itemsToKeep);
            }
        }

        [RelayCommand]
        public async Task ImportCollection()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Wybierz plik do importu",
                });

                if (result != null)
                {
                    var collectionsPath = Path.Combine(FileSystem.AppDataDirectory, "collections.txt");
                    var itemsPath = Path.Combine(FileSystem.AppDataDirectory, "items.txt");

                    var existingCollections = new List<string>();
                    if (File.Exists(collectionsPath))
                    {
                        existingCollections.AddRange(await File.ReadAllLinesAsync(collectionsPath));
                    }

                    var existingItems = new List<string>();
                    if (File.Exists(itemsPath))
                    {
                        existingItems.AddRange(await File.ReadAllLinesAsync(itemsPath));
                    }

                    var importLines = await File.ReadAllLinesAsync(result.FullPath);

                    if (importLines.Length > 0)
                    {
                        var collectionMetaData = importLines[0].Split(',');
                        var collectionId = collectionMetaData[0];
                        var collectionName = collectionMetaData.Length > 1 ? collectionMetaData[1] : "Brak nazwy";

                        if (!existingCollections.Any(c => c.StartsWith($"{collectionId},")))
                        {
                            existingCollections.Add($"{collectionId},{collectionName}");
                            Collections.Add(new SingleCollection
                            {
                                Id = collectionId,
                                Name = collectionName,
                            });
                        }
                    }

                    foreach (var line in importLines.Skip(1))
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 10)
                        {
                            if (!existingItems.Contains(line))
                            {
                                existingItems.Add(line);
                            }
                        }
                    }

                    await File.WriteAllLinesAsync(collectionsPath, existingCollections.Distinct());
                    await File.WriteAllLinesAsync(itemsPath, existingItems);

                    await Application.Current.MainPage.DisplayAlert("Import zakończony", "Kolekcje i przedmioty zostały zaimportowane pomyślnie.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd importu kolekcji: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się zaimportować kolekcji i przedmiotów.", "OK");
            }
        }


        async Task WriteCollectionsToFile()
        {
            var lines = Collections.Select(c => $"{c.Id},{c.Name}").ToList();
            await File.WriteAllLinesAsync(filePath, lines);
        }

        public async Task ReadCollectionsFromFile()
        {
            Collections.Clear();

            if (File.Exists(filePath))
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        Collections.Add(new SingleCollection { Id = parts[0], Name = parts[1] });
                    }
                }
            }
        }

    }
}
