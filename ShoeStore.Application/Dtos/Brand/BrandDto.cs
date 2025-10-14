namespace ShoeStore.Application.Dtos.Brand
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int StatusId { get; set; }
    }
}
