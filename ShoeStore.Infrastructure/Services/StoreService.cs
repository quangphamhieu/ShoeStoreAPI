// ShoeStore.Infrastructure/Services/StoreService.cs
using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Store;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class StoreService : IStoreService
    {
        private readonly ShoeStoreDbContext _db;
        public StoreService(ShoeStoreDbContext db) => _db = db;

        public async Task<IEnumerable<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _db.Stores
                .Include(s => s.Status)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return items.Select(s => new StoreDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone,
                StatusId = s.StatusId,
                StatusName = s.Status?.Name,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<StoreDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var s = await _db.Stores
                .Include(x => x.Status)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (s == null) return null;
            return new StoreDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone,
                StatusId = s.StatusId,
                StatusName = s.Status?.Name,
                CreatedAt = s.CreatedAt
            };
        }

        public async Task<int> CreateAsync(CreateStoreDto dto, CancellationToken cancellationToken = default)
        {
            var store = new Store
            {
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                StatusId = 1
            };

            _db.Stores.Add(store);
            await _db.SaveChangesAsync(cancellationToken);
            return store.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateStoreDto dto, CancellationToken cancellationToken = default)
        {
            var s = await _db.Stores.FindAsync(new object[] { id }, cancellationToken);
            if (s == null) return false;

            if (dto.Code != null) s.Code = dto.Code;
            if (dto.Name != null) s.Name = dto.Name;
            if (dto.Address != null) s.Address = dto.Address;
            if (dto.Phone != null) s.Phone = dto.Phone;
            if (dto.StatusId.HasValue) s.StatusId = dto.StatusId.Value;

            _db.Stores.Update(s);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var s = await _db.Stores.FindAsync(new object[] { id }, cancellationToken);
            if (s == null) return false;

            var inactive = await _db.Statuses.FirstOrDefaultAsync(st => st.Code.ToLower() == "inactive", cancellationToken);
            if (inactive != null)
            {
                s.StatusId = inactive.Id;
                _db.Stores.Update(s);
            }
            else
            {
                _db.Stores.Remove(s);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task<int> GetDefaultActiveStatusIdAsync(CancellationToken cancellationToken)
        {
            var active = await _db.Statuses.FirstOrDefaultAsync(s => s.Code.ToLower() == "active", cancellationToken);
            if (active != null) return active.Id;
            throw new InvalidOperationException("No Status with Code 'active' found. Please seed Status table.");
        }
    }
}
