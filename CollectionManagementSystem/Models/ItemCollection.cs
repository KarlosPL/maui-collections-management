namespace CollectionManagementSystem.Models
{
    public class ItemCollection
    {
        public string? Id { get; set; }
        public string? AssignedCollection { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public string? Status { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? ImagePath { get; set; }
        public string? CustomName { get; set; }
        public string? CustomValue { get; set; }
    }
}
