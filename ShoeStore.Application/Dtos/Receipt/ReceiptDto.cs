namespace ShoeStore.Application.Dtos.Receipt
{
    public class ReceiptDto
    {
        public long Id { get; set; }
        public string ReceiptNumber { get; set; } = null!;
        public int SupplierId { get; set; }
        public long CreatedBy { get; set; }
        public int? StoreId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }

        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public string? CreatorName { get; set; }
        public List<ReceiptDetailDto> Details { get; set; } = new();
    }
}