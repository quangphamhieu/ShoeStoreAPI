using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Brand;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class BrandService : IBrandService
    {
        private readonly ShoeStoreDbContext _context;

        public BrandService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<BrandDto>> GetAllAsync()
        {
            return await _context.Brands
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Name = b.Name,
                    Description = b.Description,
                    StatusId = b.StatusId
                })
                .ToListAsync();
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return null;

            return new BrandDto
            {
                Id = brand.Id,
                Code = brand.Code,
                Name = brand.Name,
                Description = brand.Description,
                StatusId = brand.StatusId
            };
        }

        public async Task<BrandDto> CreateAsync(CreateBrandDto dto)
        {
            var brand = new Brand
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                StatusId = 1 // Mặc định Active
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return new BrandDto
            {
                Id = brand.Id,
                Code = brand.Code,
                Name = brand.Name,
                Description = brand.Description,
                StatusId = brand.StatusId
            };
        }

        public async Task<BrandDto?> UpdateAsync(int id, UpdateBrandDto dto)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return null;

            brand.Code = dto.Code;
            brand.Name = dto.Name;
            brand.Description = dto.Description;
            brand.StatusId = dto.StatusId;

            await _context.SaveChangesAsync();

            return new BrandDto
            {
                Id = brand.Id,
                Code = brand.Code,
                Name = brand.Name,
                Description = brand.Description,
                StatusId = brand.StatusId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
