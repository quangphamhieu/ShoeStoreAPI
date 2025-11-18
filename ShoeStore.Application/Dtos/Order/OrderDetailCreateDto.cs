namespace ShoeStore.Application.Dtos.Order
{
    public class OrderDetailCreateDto
    {
        public int ProductId { get; set; }

        /// <summary>
        /// Online orders require FE to provide StoreId per item (store that owns the selected listing).
        /// Offline orders inherit the store from the parent order, so this should be left null.
        /// </summary>
        public int? StoreId { get; set; }

        public int Quantity { get; set; }
    }
}