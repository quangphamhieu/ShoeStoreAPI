using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Supplier;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ShoeStoreDbContext _context;

        public SupplierService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Status)
                .ToListAsync();

            return suppliers.Select(s => new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactInfo = s.ContactInfo,
                StatusId = s.Status.Id
            }).ToList();
        }

        public async Task<SupplierDto?> GetByIdAsync(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Status)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null) return null;

            return new SupplierDto
            {
                Id = supplier.Id,
                Code = supplier.Code,
                Name = supplier.Name,
                ContactInfo = supplier.ContactInfo,
                StatusId = supplier.Status.Id
            };
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = new Supplier
            {
                Code = dto.Code,
                Name = dto.Name,
                ContactInfo = dto.ContactInfo,
                StatusId = 1 // Active mặc định
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(supplier.Id) ?? new SupplierDto();
        }

        public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return null;

            supplier.Code = dto.Code ?? supplier.Code;
            supplier.Name = dto.Name ?? supplier.Name;
            supplier.ContactInfo = dto.ContactInfo ?? supplier.ContactInfo;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return false;

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
