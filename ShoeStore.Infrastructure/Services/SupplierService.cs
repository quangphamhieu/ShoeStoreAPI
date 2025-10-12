// ShoeStore.Infrastructure/Services/SupplierService.cs
using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Supplier;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ShoeStoreDbContext _db;
        public SupplierService(ShoeStoreDbContext db) => _db = db;

        public async Task<IEnumerable<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var items = await _db.Suppliers
                .Include(s => s.Status)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return items.Select(s => new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactInfo = s.ContactInfo,
                StatusId = s.StatusId,
                StatusName = s.Status?.Name
            });
        }

        public async Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var s = await _db.Suppliers
                .Include(x => x.Status)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (s == null) return null;
            return new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactInfo = s.ContactInfo,
                StatusId = s.StatusId,
                StatusName = s.Status?.Name
            };
        }

        public async Task<int> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default)
        {
            var supplier = new Supplier
            {
                Code = dto.Code,
                Name = dto.Name,
                ContactInfo = dto.ContactInfo,
                StatusId = 1
            };

            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync(cancellationToken);
            return supplier.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateSupplierDto dto, CancellationToken cancellationToken = default)
        {
            var s = await _db.Suppliers.FindAsync(new object[] { id }, cancellationToken);
            if (s == null) return false;

            if (dto.Code != null) s.Code = dto.Code;
            if (dto.Name != null) s.Name = dto.Name;
            if (dto.ContactInfo != null) s.ContactInfo = dto.ContactInfo;
            if (dto.StatusId.HasValue) s.StatusId = dto.StatusId.Value;

            _db.Suppliers.Update(s);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var s = await _db.Suppliers.FindAsync(new object[] { id }, cancellationToken);
            if (s == null) return false;

            var inactive = await _db.Statuses.FirstOrDefaultAsync(st => st.Code.ToLower() == "inactive", cancellationToken);
            if (inactive != null)
            {
                s.StatusId = inactive.Id;
                _db.Suppliers.Update(s);
            }
            else
            {
                _db.Suppliers.Remove(s);
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
