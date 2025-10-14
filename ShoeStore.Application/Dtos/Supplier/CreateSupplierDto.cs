namespace ShoeStore.Application.Dtos.Supplier
{
    public class CreateSupplierDto
    {
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? ContactInfo { get; set; }
    }
}
