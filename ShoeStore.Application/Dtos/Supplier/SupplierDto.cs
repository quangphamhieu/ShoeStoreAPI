namespace ShoeStore.Application.Dtos.Supplier
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? ContactInfo { get; set; }
        public int StatusId { get; set; }
    }
}
