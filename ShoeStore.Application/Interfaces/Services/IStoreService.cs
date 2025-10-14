using ShoeStore.Application.Dtos.Store;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IStoreService
    {
        Task<List<StoreDto>> GetAllAsync();
        Task<StoreDto?> GetByIdAsync(int id);
        Task<StoreDto> CreateAsync(CreateStoreDto dto);
        Task<StoreDto?> UpdateAsync(int id, UpdateStoreDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
