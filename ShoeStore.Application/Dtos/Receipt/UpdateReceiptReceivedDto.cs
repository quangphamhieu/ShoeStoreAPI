namespace ShoeStore.Application.Dtos.Receipt
{
    public class UpdateReceiptReceivedDto
    {
        // toàn bộ danh sách detail cập nhật ReceivedQuantity
        public List<UpdateReceivedDetailDto> Details { get; set; } = new();
    }

    public class UpdateReceivedDetailDto
    {
        public long ReceiptDetailId { get; set; } // hoặc ProductId -> dùng ReceiptDetailId an toàn hơn
        public int ReceivedQuantity { get; set; }
    }
}
