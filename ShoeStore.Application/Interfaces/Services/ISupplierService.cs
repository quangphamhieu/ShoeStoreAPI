using ShoeStore.Application.Dtos.Supplier;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(int id, UpdateSupplierDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
