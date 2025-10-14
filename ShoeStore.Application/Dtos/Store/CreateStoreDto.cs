namespace ShoeStore.Application.Dtos.Store
{
    public class CreateStoreDto
    {
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
}
