namespace ShoeStore.Application.Dtos.Receipt
{
    public class CreateReceiptDto
    {
        public int SupplierId { get; set; }
        public int? StoreId { get; set; }
        public List<CreateReceiptDetailDto> Details { get; set; } = new();
        // CreatedBy is defaulted to 1 in service (you can allow override if wanted)
    }

    public class CreateReceiptDetailDto
    {
        public int ProductId { get; set; }
        public int QuantityOrdered { get; set; }
        // UnitPrice will be looked up from product.CostPrice
    }
}
