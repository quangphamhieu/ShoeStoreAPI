using ShoeStore.Application.Dtos.Product;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ProductDto>> SearchAsync(SearchProductDto searchDto);
        Task<IEnumerable<string>> SuggestAsync(string keyword);

        // 🔹 API cho bảng trung gian Product-Store
        Task<StoreQuantityDto?> CreateStoreQuantityAsync(StoreQuantityDto dto, int productId);
        Task<StoreQuantityDto?> UpdateStoreQuantityAsync(StoreQuantityDto dto, int productId);
    }
}
