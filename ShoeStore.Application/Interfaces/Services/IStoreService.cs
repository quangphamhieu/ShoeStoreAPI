using ShoeStore.Application.Dtos.Store;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StoreDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateStoreDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id, UpdateStoreDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
