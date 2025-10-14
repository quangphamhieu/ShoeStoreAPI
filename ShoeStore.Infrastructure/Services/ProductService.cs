using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Product;
using ShoeStore.Application.Interfaces;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ShoeStoreDbContext _context;

        public ProductService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateSkuAsync(Product product)
        {
            string brandCode = "GEN"; // default generic code
            if (product.BrandId.HasValue)
            {
                var brand = await _context.Brands.FindAsync(product.BrandId);
                if (brand != null && !string.IsNullOrEmpty(brand.Code))
                    brandCode = brand.Code;
            }

            string namePart = product.Name.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
            string colorPart = product.Color ?? "NA";
            string sizePart = product.Size ?? "NA";

            return $"{brandCode}-{namePart}-{colorPart}-{sizePart}".ToUpper();
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            return await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    SKU = p.SKU,
                    Name = p.Name,
                    BrandId = p.BrandId,
                    SupplierId = p.SupplierId,
                    StoreId = p.StoreId,
                    CostPrice = p.CostPrice,
                    SalePrice = p.SalePrice,
                    Color = p.Color,
                    Size = p.Size,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    StatusId = p.StatusId,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return null;

            return new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                BrandId = p.BrandId,
                SupplierId = p.SupplierId,
                StoreId = p.StoreId,
                CostPrice = p.CostPrice,
                SalePrice = p.SalePrice,
                Color = p.Color,
                Size = p.Size,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                BrandId = dto.BrandId,
                SupplierId = dto.SupplierId,
                StoreId = dto.StoreId,
                CostPrice = dto.CostPrice,
                SalePrice = dto.SalePrice,
                Color = dto.Color,
                Size = dto.Size,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow
            };


            // Add first to generate ID if needed
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Generate SKU
            product.SKU = await GenerateSkuAsync(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                StoreId = product.StoreId,
                CostPrice = product.CostPrice,
                SalePrice = product.SalePrice,
                Color = product.Color,
                Size = product.Size,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                StatusId = product.StatusId,
                CreatedAt = product.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Name = dto.Name;
            product.BrandId = dto.BrandId;
            product.SupplierId = dto.SupplierId;
            product.StoreId = dto.StoreId;
            product.CostPrice = dto.CostPrice;
            product.SalePrice = dto.SalePrice;
            product.Color = dto.Color;
            product.Size = dto.Size;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            product.StatusId = dto.StatusId;

            // Update SKU too if key attributes changed
            product.SKU = await GenerateSkuAsync(product);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(SearchProductDto searchDto)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.Name))
                query = query.Where(p => p.Name.Contains(searchDto.Name));

            if (!string.IsNullOrEmpty(searchDto.Color))
                query = query.Where(p => p.Color == searchDto.Color);

            if (!string.IsNullOrEmpty(searchDto.Size))
                query = query.Where(p => p.Size == searchDto.Size);

            if (searchDto.MinPrice.HasValue)
                query = query.Where(p => p.SalePrice >= searchDto.MinPrice.Value);

            if (searchDto.MaxPrice.HasValue)
                query = query.Where(p => p.SalePrice <= searchDto.MaxPrice.Value);

            return await query.Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                BrandId = p.BrandId,
                SupplierId = p.SupplierId,
                StoreId = p.StoreId,
                CostPrice = p.CostPrice,
                SalePrice = p.SalePrice,
                Color = p.Color,
                Size = p.Size,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt
            }).ToListAsync();
        }

        public async Task<IEnumerable<string>> SuggestAsync(string keyword)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(keyword))
                .Select(p => p.Name)
                .Distinct()
                .Take(10)
                .ToListAsync();
        }
    }
}
