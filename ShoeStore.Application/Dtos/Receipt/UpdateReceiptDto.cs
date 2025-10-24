namespace ShoeStore.Application.Dtos.Receipt
{
    public class UpdateReceiptDto
    {
        public int SupplierId { get; set; }
        public int? StoreId { get; set; }
        public List<UpdateReceiptDetailDto> Details { get; set; } = new();
    }

    public class UpdateReceiptDetailDto
    {
        public int ProductId { get; set; }
        public int QuantityOrdered { get; set; }
    }
}
