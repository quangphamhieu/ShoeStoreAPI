using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Product;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;
using System.Collections.Generic;

namespace ShoeStore.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ShoeStoreDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductService(ShoeStoreDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // 🔹 Generate SKU: BRANDCODE-NAME-COLOR-SIZE
        private async Task<string> GenerateSkuAsync(Product product)
        {
            string brandCode = "GEN";

            if (product.BrandId.HasValue)
            {
                var brand = await _context.Brands
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == product.BrandId.Value);

                if (brand?.Code != null)
                    brandCode = brand.Code;
            }

            string namePart = product.Name.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
            string colorPart = string.IsNullOrWhiteSpace(product.Color) ? "NA" : product.Color;
            string sizePart = string.IsNullOrWhiteSpace(product.Size) ? "NA" : product.Size;

            return $"{brandCode}-{namePart}-{colorPart}-{sizePart}".ToUpperInvariant();
        }

        // 🔹 Lấy toàn bộ sản phẩm kèm store
        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.StoreProducts!)
                    .ThenInclude(sp => sp.Store)
                .AsNoTracking()
                .ToListAsync();

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU ?? string.Empty,
                Name = p.Name,
                BrandId = p.BrandId,
                SupplierId = p.SupplierId,
                CostPrice = p.CostPrice,
                OriginalPrice = p.OriginalPrice,
                Color = p.Color,
                Size = p.Size,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt,
                Stores = p.StoreProducts?.Select(sp => new StoreQuantityDto
                {
                    StoreId = sp.StoreId,
                    StoreName = sp.Store?.Name ?? string.Empty,
                    Quantity = sp.Quantity,
                    SalePrice = sp.SalePrice
                })
                            .ToList() ?? new List<StoreQuantityDto>()

            }).ToList();
        }

        // 🔹 Lấy chi tiết 1 sản phẩm
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var p = await _context.Products
                .Include(x => x.StoreProducts!)
                    .ThenInclude(sp => sp.Store)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU ?? string.Empty,
                Name = p.Name,
                BrandId = p.BrandId,
                SupplierId = p.SupplierId,
                CostPrice = p.CostPrice,
                OriginalPrice = p.OriginalPrice,
                Color = p.Color,
                Size = p.Size,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt,
                Stores = p.StoreProducts?.Select(sp => new StoreQuantityDto
                {
                    StoreId = sp.StoreId,
                    StoreName = sp.Store?.Name ?? string.Empty,
                    Quantity = sp.Quantity,
                    SalePrice = sp.SalePrice
                })
                    .ToList() ?? new List<StoreQuantityDto>()
            };
        }

        // 🔹 Tạo sản phẩm
        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name ?? string.Empty,
                BrandId = dto.BrandId,
                SupplierId = dto.SupplierId,
                CostPrice = dto.CostPrice,
                OriginalPrice = dto.OriginalPrice,
                Color = dto.Color,
                Size = dto.Size,
                Description = dto.Description,
                ImageUrl = dto.ImageFile != null ? await _cloudinaryService.UploadImageAsync(dto.ImageFile) : dto.ImageUrl,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            product.SKU = await GenerateSkuAsync(product);
            await _context.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                CostPrice = product.CostPrice,
                OriginalPrice = product.OriginalPrice,
                Color = product.Color,
                Size = product.Size,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                StatusId = product.StatusId,
                CreatedAt = product.CreatedAt
            };
        }

        // 🔹 Cập nhật sản phẩm
        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            product.Name = dto.Name;
            product.BrandId = dto.BrandId;
            product.SupplierId = dto.SupplierId;

            var oldOriginalPrice = product.OriginalPrice;

            product.CostPrice = dto.CostPrice;
            product.OriginalPrice = dto.OriginalPrice;
            product.Color = dto.Color;
            product.Size = dto.Size;
            product.Description = dto.Description;
            if (dto.ImageFile != null)
            {
                product.ImageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
            }
            else
            {
                product.ImageUrl = dto.ImageUrl;
            }
            ;
            product.StatusId = dto.StatusId;
            product.SKU = await GenerateSkuAsync(product);

            if (oldOriginalPrice != product.OriginalPrice)
            {
                await UpdateStoreProductsSalePriceFromOriginalAsync(product.Id, oldOriginalPrice, product.OriginalPrice);
            }

            await _context.SaveChangesAsync();
            return new ProductDto
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                CostPrice = product.CostPrice,
                OriginalPrice = product.OriginalPrice,
                Color = product.Color,
                Size = product.Size,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                StatusId = product.StatusId,
                CreatedAt = product.CreatedAt
            };
        }

        // 🔹 Xóa sản phẩm
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // 🔹 Tìm kiếm sản phẩm
        public async Task<IEnumerable<ProductDto>> SearchAsync(SearchProductDto searchDto)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchDto.Name))
                query = query.Where(p => p.Name.Contains(searchDto.Name));

            if (!string.IsNullOrWhiteSpace(searchDto.Color))
                query = query.Where(p => p.Color == searchDto.Color);

            if (!string.IsNullOrWhiteSpace(searchDto.Size))
                query = query.Where(p => p.Size == searchDto.Size);

            var products = await query
                .Include(p => p.StoreProducts!)
                    .ThenInclude(sp => sp.Store)
                .AsNoTracking()
                .ToListAsync();

            // Filter by price if needed (check StoreProduct.SalePrice)
            if (searchDto.MinPrice.HasValue || searchDto.MaxPrice.HasValue)
            {
                products = products.Where(p =>
                    p.StoreProducts != null &&
                    p.StoreProducts.Any(sp =>
                        (!searchDto.MinPrice.HasValue || sp.SalePrice >= searchDto.MinPrice.Value) &&
                        (!searchDto.MaxPrice.HasValue || sp.SalePrice <= searchDto.MaxPrice.Value)
                    )
                ).ToList();
            }

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU ?? string.Empty,
                Name = p.Name,
                BrandId = p.BrandId,
                SupplierId = p.SupplierId,
                CostPrice = p.CostPrice,
                OriginalPrice = p.OriginalPrice,
                Color = p.Color,
                Size = p.Size,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt,
                Stores = p.StoreProducts?.Select(sp => new StoreQuantityDto
                {
                    StoreId = sp.StoreId,
                    StoreName = sp.Store?.Name ?? string.Empty,
                    Quantity = sp.Quantity,
                    SalePrice = sp.SalePrice
                }).ToList() ?? new List<StoreQuantityDto>()
            }).ToList();
        }

        // 🔹 Gợi ý tên sản phẩm
        public async Task<IEnumerable<string>> SuggestAsync(string keyword)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(keyword))
                .Select(p => p.Name)
                .Distinct()
                .Take(10)
                .ToListAsync();
        }

        // 🔹 Tạo mới Product-Store (ví dụ khi tạo cửa hàng)
        public async Task<StoreQuantityDto?> CreateStoreQuantityAsync(StoreQuantityDto dto, int productId)
        {
            var exists = await _context.StoreProducts
                .AnyAsync(x => x.ProductId == productId && x.StoreId == dto.StoreId);

            if (exists) return null;

            // Lấy product để lấy OriginalPrice
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            var salePrice = dto.SalePrice ?? product.OriginalPrice;

            var entity = new StoreProduct
            {
                ProductId = productId,
                StoreId = dto.StoreId,
                Quantity = dto.Quantity,
                SalePrice = salePrice
            };

            _context.StoreProducts.Add(entity);
            await _context.SaveChangesAsync();

            var store = await _context.Stores.FindAsync(dto.StoreId);

            return new StoreQuantityDto
            {
                StoreId = entity.StoreId,
                StoreName = store?.Name ?? string.Empty,
                Quantity = entity.Quantity,
                SalePrice = entity.SalePrice
            };
        }

        // 🔹 Cập nhật số lượng sản phẩm tại 1 cửa hàng
        public async Task<StoreQuantityDto?> UpdateStoreQuantityAsync(StoreQuantityDto dto, int productId)
        {
            var ps = await _context.StoreProducts
                .Include(sp => sp.Store)
                .FirstOrDefaultAsync(sp => sp.ProductId == productId && sp.StoreId == dto.StoreId);

            if (ps == null) return null;

            ps.Quantity = dto.Quantity;

            if (dto.SalePrice.HasValue)
            {
                ps.SalePrice = dto.SalePrice.Value;
            }

            await _context.SaveChangesAsync();

            return new StoreQuantityDto
            {
                StoreId = ps.StoreId,
                StoreName = ps.Store?.Name ?? string.Empty,
                Quantity = ps.Quantity,
                SalePrice = ps.SalePrice
            };
        }

        private async Task UpdateStoreProductsSalePriceFromOriginalAsync(int productId, decimal oldOriginalPrice, decimal newOriginalPrice)
        {
            if (oldOriginalPrice == newOriginalPrice)
                return;

            var storeProducts = await _context.StoreProducts
                .Where(sp => sp.ProductId == productId)
                .ToListAsync();

            if (!storeProducts.Any())
                return;

            var activePromotionIds = await _context.PromotionProducts
                .Where(pp => pp.ProductId == productId)
                .Join(_context.Promotions,
                    pp => pp.PromotionId,
                    promotion => promotion.Id,
                    (pp, promotion) => new { pp, promotion })
                .Where(x => x.promotion.StatusId == 1
                            && x.promotion.StartDate <= DateTime.UtcNow
                            && x.promotion.EndDate >= DateTime.UtcNow)
                .Select(x => x.pp.PromotionId)
                .Distinct()
                .ToListAsync();

            var activeStoreIds = activePromotionIds.Any()
                ? await _context.PromotionStores
                    .Where(ps => activePromotionIds.Contains(ps.PromotionId))
                    .Select(ps => ps.StoreId)
                    .Distinct()
                    .ToListAsync()
                : new List<int>();

            var activeStoreSet = activeStoreIds.Count > 0 ? new HashSet<int>(activeStoreIds) : null;

            foreach (var sp in storeProducts)
            {
                if (activeStoreSet != null && activeStoreSet.Contains(sp.StoreId))
                    continue;

                // Chỉ cập nhật những store đang dùng giá gốc cũ (tránh ghi đè giá tùy chỉnh)
                if (sp.SalePrice == oldOriginalPrice)
                {
                    sp.SalePrice = newOriginalPrice;
                }
            }
        }
    }
}
