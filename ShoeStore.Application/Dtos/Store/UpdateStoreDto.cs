namespace ShoeStore.Application.Dtos.Store
{
    public class UpdateStoreDto
    {
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int StatusId { get; set; }
    }
}
