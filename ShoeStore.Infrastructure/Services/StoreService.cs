using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Store;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class StoreService : IStoreService
    {
        private readonly ShoeStoreDbContext _context;

        public StoreService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<StoreDto>> GetAllAsync()
        {
            var stores = await _context.Stores
                .Include(s => s.Status)
                .ToListAsync();

            return stores.Select(s => new StoreDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone,
                CreatedAt = s.CreatedAt,
                StatusId = s.Status.Id
            }).ToList();
        }

        public async Task<StoreDto?> GetByIdAsync(int id)
        {
            var store = await _context.Stores
                .Include(s => s.Status)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null) return null;

            return new StoreDto
            {
                Id = store.Id,
                Code = store.Code,
                Name = store.Name,
                Address = store.Address,
                Phone = store.Phone,
                CreatedAt = store.CreatedAt,
                StatusId = store.Status.Id
            };
        }

        public async Task<StoreDto> CreateAsync(CreateStoreDto dto)
        {
            var store = new Store
            {
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                StatusId = 1 // mặc định active
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(store.Id) ?? new StoreDto();
        }

        public async Task<StoreDto?> UpdateAsync(int id, UpdateStoreDto dto)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return null;

            store.Code = dto.Code ?? store.Code;
            store.Name = dto.Name ?? store.Name;
            store.Address = dto.Address ?? store.Address;
            store.Phone = dto.Phone ?? store.Phone;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return false;

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
