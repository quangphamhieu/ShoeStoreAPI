namespace ShoeStore.Domain.Entities
{
    public class ReceiptDetail
    {
        public long Id { get; set; }
        public long ReceiptId { get; set; }
        public int ProductId { get; set; }
        public int QuantityOrdered { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        public Receipt Receipt { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
