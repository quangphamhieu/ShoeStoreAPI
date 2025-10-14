namespace ShoeStore.Application.Dtos.Brand
{
    public class CreateBrandDto
    {
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
