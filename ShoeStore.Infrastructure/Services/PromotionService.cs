using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Notification;
using ShoeStore.Application.Dtos.Promotion;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly ShoeStoreDbContext _context;

        private readonly INotificationService _notificationService;

        public PromotionService(ShoeStoreDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        // ✅ GET ALL
        public async Task<IEnumerable<PromotionDto>> GetAllAsync()
        {
            var promotions = await _context.Promotions
                .Include(p => p.Status)
                .Include(p => p.PromotionProducts)!.ThenInclude(pp => pp.Product)
                .Include(p => p.PromotionStores)!.ThenInclude(ps => ps.Store)
                .ToListAsync();

            return promotions.Select(p => new PromotionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                StatusId = p.StatusId,
                StatusName = p.Status?.Name,
                Products = p.PromotionProducts?.Select(pp => new PromotionProductDto
                {
                    ProductId = pp.ProductId,
                    ProductName = pp.Product.Name,
                    SKU = pp.Product.SKU,
                    SalePrice = pp.Product.SalePrice,
                    DiscountPercent = pp.DiscountPercent
                }).ToList() ?? new List<PromotionProductDto>(),
                Stores = p.PromotionStores?.Select(ps => new PromotionStoreDto
                {
                    StoreId = ps.StoreId,
                    StoreName = ps.Store?.Name
                }).ToList() ?? new List<PromotionStoreDto>()
            });
        }


        // ✅ GET BY ID
        public async Task<PromotionDto?> GetByIdAsync(int id)
        {
            var p = await _context.Promotions
                .Include(p => p.Status)
                .Include(p => p.PromotionProducts)!.ThenInclude(pp => pp.Product)
                .Include(p => p.PromotionStores)!.ThenInclude(ps => ps.Store)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return new PromotionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                StatusId = p.StatusId,
                StatusName = p.Status?.Name,
                Products = p.PromotionProducts?.Select(pp => new PromotionProductDto
                {
                    ProductId = pp.ProductId,
                    ProductName = pp.Product.Name,
                    SKU = pp.Product.SKU,
                    SalePrice = pp.Product.SalePrice,
                    DiscountPercent = pp.DiscountPercent
                }).ToList(),
                Stores = p.PromotionStores?.Select(ps => new PromotionStoreDto
                {
                    StoreId = ps.StoreId,
                    StoreName = ps.Store?.Name
                }).ToList()
            };
        }
        // ⚙️ Áp dụng giảm giá
        private async Task ApplyDiscountToProductsAsync(Promotion promotion)
        {
            foreach (var pp in promotion.PromotionProducts!)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == pp.ProductId);
                if (product == null) continue;

                // Tính giảm giá dựa trên OriginalPrice (luôn có giá trị)
                var discountAmount = product.OriginalPrice * (pp.DiscountPercent / 100);
                product.SalePrice = Math.Round(product.OriginalPrice - discountAmount, 2);
            }

            await _context.SaveChangesAsync();
        }

        // ⚙️ Khôi phục giá gốc
        private async Task RestoreOriginalPricesAsync(Promotion promotion)
        {
            foreach (var pp in promotion.PromotionProducts!)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == pp.ProductId);
                if (product == null) continue;

                // Khôi phục lại giá gốc (SalePrice = OriginalPrice)
                product.SalePrice = product.OriginalPrice;
            }

            await _context.SaveChangesAsync();
        }

        private string BuildProductsSummary(Promotion promotion)
        {
            if (promotion.PromotionProducts == null || !promotion.PromotionProducts.Any())
                return "Không có sản phẩm áp dụng.";

            var parts = promotion.PromotionProducts.Select(pp =>
            {
                var prod = pp.Product;
                var name = prod?.Name ?? $"ProductId:{pp.ProductId}";
                var sku = prod?.SKU;
                return $"{(string.IsNullOrWhiteSpace(sku) ? name : $"{sku} - {name}")}: {pp.DiscountPercent}%";
            });

            return string.Join("; ", parts);
        }


        // ✅ CREATE
        public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto)
        {
            var now = DateTime.Now;
            var code = $"KM-{now:yyyyMMddHHmmss}";

            var promotion = new Promotion
            {
                Code = code,
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                StatusId = dto.StartDate <= now ? 1 : 2,
                PromotionProducts = dto.Products?.Select(p => new PromotionProduct
                {
                    ProductId = p.ProductId,
                    DiscountPercent = p.DiscountPercent
                }).ToList(),
                PromotionStores = dto.Stores?.Select(s => new PromotionStore
                {
                    StoreId = s.StoreId
                }).ToList()
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            promotion = await _context.Promotions
                .Include(p => p.PromotionProducts)!.ThenInclude(pp => pp.Product)
                .FirstOrDefaultAsync(p => p.Id == promotion.Id) ?? promotion;

            // Build notification message
            var productsSummary = BuildProductsSummary(promotion);
            string timePart;
            if (promotion.StatusId == 1)
            {
                timePart = "Chương trình đã bắt đầu.";
            }
            else
            {
                var days = (promotion.StartDate.Date - now.Date).Days;
                timePart = days > 0 ? $"Còn {days} ngày nữa đến khi chương trình bắt đầu." : "Sắp bắt đầu.";
            }

            var message = $"{promotion.Name} | {promotion.StartDate:dd/MM/yyyy} - {promotion.EndDate:dd/MM/yyyy}. {timePart} Sản phẩm: {productsSummary}";

            await _notificationService.CreateAsync(new CreateNotificationDto
            {
                Title = promotion.StatusId == 1 ? $"Khuyến mãi bắt đầu: {promotion.Name}" : $"Khuyến mãi sắp tới: {promotion.Name}",
                Message = message,
                Type = "Promotion"
            });




            // Nếu đang active thì giảm giá
            if (promotion.StatusId == 1)
                await ApplyDiscountToProductsAsync(promotion);

            return await GetByIdAsync(promotion.Id) ?? new PromotionDto();
        }

        // ✅ UPDATE
        public async Task<PromotionDto?> UpdateAsync(int id, UpdatePromotionDto dto)
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionStores)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (promotion == null) return null;

            var now = DateTime.Now;

            // cập nhật cơ bản
            promotion.Name = dto.Name;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;

            // kiểm tra trạng thái theo thời gian
            if (now >= promotion.StartDate && now <= promotion.EndDate)
                promotion.StatusId = 1; // active
            else
                promotion.StatusId = 2; // inactive

            // cập nhật danh sách product
            _context.PromotionProducts.RemoveRange(promotion.PromotionProducts!);
            promotion.PromotionProducts = dto.Products?.Select(p => new PromotionProduct
            {
                ProductId = p.ProductId,
                DiscountPercent = p.DiscountPercent,
                PromotionId = id
            }).ToList();

            // cập nhật danh sách store
            _context.PromotionStores.RemoveRange(promotion.PromotionStores!);
            promotion.PromotionStores = dto.Stores?.Select(s => new PromotionStore
            {
                PromotionId = id,
                StoreId = s.StoreId
            }).ToList();

            await _context.SaveChangesAsync();

            // xử lý giá theo trạng thái
            promotion = await _context.Promotions
                .Include(p => p.PromotionProducts)!.ThenInclude(pp => pp.Product)
                .Include(p => p.PromotionStores)!.ThenInclude(ps => ps.Store)
                .FirstOrDefaultAsync(p => p.Id == id) ?? promotion;

            // xử lý giá theo trạng thái & prepare notification
            var productsSummary = BuildProductsSummary(promotion);
            if (promotion.StatusId == 1)
            {
                // active -> apply discount
                await ApplyDiscountToProductsAsync(promotion);

                var msg = $"{promotion.Name} đã bắt đầu. Thời gian: {promotion.StartDate:dd/MM/yyyy} - {promotion.EndDate:dd/MM/yyyy}. Sản phẩm: {productsSummary}";
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    Title = $"Khuyến mãi bắt đầu: {promotion.Name}",
                    Message = msg,
                    Type = "Promotion"
                });
            }
            else
            {
                // inactive
                if (now > promotion.EndDate)
                {
                    // vừa kết thúc -> restore prices, notify end
                    await RestoreOriginalPricesAsync(promotion);
                    var msg = $"{promotion.Name} đã kết thúc vào {promotion.EndDate:dd/MM/yyyy}.";
                    if (!string.IsNullOrWhiteSpace(productsSummary))
                        msg += $" Sản phẩm áp dụng trước đó: {productsSummary}";
                    await _notificationService.CreateAsync(new CreateNotificationDto
                    {
                        Title = $"Khuyến mãi kết thúc: {promotion.Name}",
                        Message = msg,
                        Type = "Promotion"
                    });
                }
                else
                {
                    // start in future -> notify countdown
                    var days = (promotion.StartDate.Date - now.Date).Days;
                    var dayText = days > 0 ? $"Còn {days} ngày nữa đến khi chương trình bắt đầu." : "Sắp bắt đầu.";
                    var msg = $"{promotion.Name} | {promotion.StartDate:dd/MM/yyyy} - {promotion.EndDate:dd/MM/yyyy}. {dayText} Sản phẩm: {productsSummary}";
                    await _notificationService.CreateAsync(new CreateNotificationDto
                    {
                        Title = $"Khuyến mãi cập nhật: {promotion.Name}",
                        Message = msg,
                        Type = "Promotion"
                    });
                }
            }

            return await GetByIdAsync(id);
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionStores)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null) return false;

            _context.PromotionProducts.RemoveRange(promotion.PromotionProducts!);
            _context.PromotionStores.RemoveRange(promotion.PromotionStores!);
            _context.Promotions.Remove(promotion);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
