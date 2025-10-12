// ShoeStore.Infrastructure/Services/BrandService.cs
using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Brand;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services;
public class BrandService : IBrandService
{
    private readonly ShoeStoreDbContext _db;
    public BrandService(ShoeStoreDbContext db) => _db = db;

    public async Task<IEnumerable<BrandDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _db.Brands
            .Include(b => b.Status)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return brands.Select(b => new BrandDto
        {
            Id = b.Id,
            Code = b.Code,
            Name = b.Name,
            Description = b.Description,
            StatusId = b.StatusId,
            StatusName = b.Status?.Name
        });
    }

    public async Task<BrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var b = await _db.Brands
            .Include(x => x.Status)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (b == null) return null;
        return new BrandDto
        {
            Id = b.Id,
            Code = b.Code,
            Name = b.Name,
            Description = b.Description,
            StatusId = b.StatusId,
            StatusName = b.Status?.Name
        };
    }

    public async Task<int> CreateAsync(CreateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var brand = new Brand
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            StatusId = 1
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync(cancellationToken);
        return brand.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var brand = await _db.Brands.FindAsync(new object[] { id }, cancellationToken);
        if (brand == null) return false;

        if (dto.Code != null) brand.Code = dto.Code;
        if (dto.Name != null) brand.Name = dto.Name;
        if (dto.Description != null) brand.Description = dto.Description;
        if (dto.StatusId.HasValue) brand.StatusId = dto.StatusId.Value;

        _db.Brands.Update(brand);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await _db.Brands.FindAsync(new object[] { id }, cancellationToken);
        if (brand == null) return false;

        // Soft-delete by setting Status = inactive (if exists). Otherwise perform hard delete.
        var inactive = await _db.Statuses.FirstOrDefaultAsync(s => s.Code.ToLower() == "inactive", cancellationToken);
        if (inactive != null)
        {
            brand.StatusId = inactive.Id;
            _db.Brands.Update(brand);
        }
        else
        {
            _db.Brands.Remove(brand);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int> GetDefaultActiveStatusIdAsync(CancellationToken cancellationToken)
    {
        var active = await _db.Statuses.FirstOrDefaultAsync(s => s.Code.ToLower() == "active", cancellationToken);
        if (active != null) return active.Id;

        // fallback: take any status or create default? here throw to make config explicit
        throw new InvalidOperationException("No Status with Code 'active' found. Please seed Status table.");
    }
}
