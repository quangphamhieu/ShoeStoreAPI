// ShoeStore.Application/Interfaces/Services/IBrandService.cs
using ShoeStore.Application.Dtos.Brand;

namespace ShoeStore.Application.Interfaces.Services;
public interface IBrandService
{
    Task<IEnumerable<BrandDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateBrandDto dto, CancellationToken cancellationToken = default); // returns new id
    Task<bool> UpdateAsync(int id, UpdateBrandDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default); // soft-delete preferred (set inactive), returns success
}

// Similar interfaces for ISupplierService and IStoreService
