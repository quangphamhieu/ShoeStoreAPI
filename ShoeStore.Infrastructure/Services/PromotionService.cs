using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Notification;
using ShoeStore.Application.Dtos.Promotion;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
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
                    SalePrice = null, // SalePrice giờ ở StoreProduct, không ở Product
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
                    SalePrice = null, // SalePrice giờ ở StoreProduct, không ở Product
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
        private async Task ApplyDiscountToProductsAsync(Promotion promotion, IEnumerable<int>? overrideStoreIds = null, IEnumerable<PromotionProduct>? overrideProducts = null)
        {
            var storeIds = overrideStoreIds?.ToList()
                ?? promotion.PromotionStores?.Select(ps => ps.StoreId).ToList()
                ?? new List<int>();

            if (!storeIds.Any()) return;

            var promotionProducts = overrideProducts?.ToList() ?? promotion.PromotionProducts?.ToList() ?? new List<PromotionProduct>();
            if (!promotionProducts.Any()) return;

            var now = DateTime.Now;
            if (promotion.StartDate > now || promotion.EndDate < now) return;

            foreach (var pp in promotionProducts)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == pp.ProductId);
                if (product == null) continue;

                // Tính giảm giá dựa trên OriginalPrice (luôn có giá trị)
                var discountAmount = product.OriginalPrice * (pp.DiscountPercent / 100);
                var newSalePrice = Math.Round(product.OriginalPrice - discountAmount, 2);

                // Cập nhật SalePrice cho StoreProduct của các store trong promotion
                var storeProducts = await _context.StoreProducts
                    .Where(sp => sp.ProductId == pp.ProductId && storeIds.Contains(sp.StoreId))
                    .ToListAsync();

                foreach (var sp in storeProducts)
                {
                    sp.SalePrice = newSalePrice;
                }
            }

            await _context.SaveChangesAsync();
        }

        // ⚙️ Khôi phục giá gốc
        private async Task RestoreOriginalPricesAsync(Promotion promotion, IEnumerable<int>? overrideStoreIds = null, IEnumerable<PromotionProduct>? overrideProducts = null)
        {
            var storeIds = overrideStoreIds?.ToList()
                ?? promotion.PromotionStores?.Select(ps => ps.StoreId).ToList()
                ?? new List<int>();

            if (!storeIds.Any()) return;

            var promotionProducts = overrideProducts?.ToList() ?? promotion.PromotionProducts?.ToList() ?? new List<PromotionProduct>();
            if (!promotionProducts.Any()) return;

            foreach (var pp in promotionProducts)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == pp.ProductId);
                if (product == null) continue;

                // Khôi phục lại giá gốc (SalePrice = OriginalPrice) cho StoreProduct
                var storeProducts = await _context.StoreProducts
                    .Where(sp => sp.ProductId == pp.ProductId && storeIds.Contains(sp.StoreId))
                    .ToListAsync();

                foreach (var sp in storeProducts)
                {
                    sp.SalePrice = product.OriginalPrice;
                }
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
                return $"• {(string.IsNullOrWhiteSpace(sku) ? name : $"{sku} - {name}")}: Giảm {pp.DiscountPercent}%";
            });

            return string.Join("\n", parts);
        }

        private async Task ValidateStorePromotionAvailabilityAsync(IEnumerable<int>? storeIds, int? excludePromotionId = null)
        {
            if (storeIds == null) return;
            var list = storeIds.Distinct().ToList();
            if (!list.Any()) return;

            var now = DateTime.Now;

            var conflicts = await _context.PromotionStores
                .Include(ps => ps.Promotion)
                .Where(ps => list.Contains(ps.StoreId))
                .Where(ps => ps.PromotionId != excludePromotionId)
                .Where(ps => ps.Promotion.StatusId == 1
                             && ps.Promotion.StartDate <= now
                             && ps.Promotion.EndDate >= now)
                .Select(ps => new { ps.StoreId, PromotionName = ps.Promotion.Name })
                .ToListAsync();

            if (conflicts.Any())
            {
                var conflictStores = string.Join(", ", conflicts.Select(c => $"StoreId {c.StoreId} (Promotion: {c.PromotionName})"));
                throw new InvalidOperationException($"Cửa hàng đang có khuyến mãi hoạt động: {conflictStores}. Vui lòng kết thúc khuyến mãi trước khi tạo mới.");
            }
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

            var message = $"🎉 Chương trình khuyến mãi đặc biệt!\n\n" +
                $"📅 Thời gian: {promotion.StartDate:dd/MM/yyyy} - {promotion.EndDate:dd/MM/yyyy}\n" +
                $"⏰ {timePart}\n\n" +
                $"🛍️ Sản phẩm áp dụng:\n{productsSummary}";

            await _notificationService.CreateAsync(new CreateNotificationDto
            {
                Title = promotion.StatusId == 1 ? $"Khuyến mãi bắt đầu: {promotion.Name}" : $"Khuyến mãi sắp tới: {promotion.Name}",
                Message = message,
                Type = "Promotion"
            });




            // Nếu đang active thì giảm giá
            if (promotion.StatusId == 1)
            {
                await ValidateStorePromotionAvailabilityAsync(promotion.PromotionStores?.Select(s => s.StoreId));
                await ApplyDiscountToProductsAsync(promotion);
            }

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
            var previousStatusId = promotion.StatusId;
            var previousStoreIds = promotion.PromotionStores?.Select(ps => ps.StoreId).Distinct().ToList() ?? new List<int>();
            var existingProductsSnapshot = promotion.PromotionProducts?
                .Select(pp => new PromotionProduct
                {
                    PromotionId = id,
                    ProductId = pp.ProductId,
                    DiscountPercent = pp.DiscountPercent
                })
                .ToList() ?? new List<PromotionProduct>();

            promotion.Name = dto.Name;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;

            // kiểm tra trạng thái theo thời gian
            if (now >= promotion.StartDate && now <= promotion.EndDate)
                promotion.StatusId = 1; // active
            else
                promotion.StatusId = 2; // inactive

            // cập nhật danh sách product
            var existingProducts = promotion.PromotionProducts?.ToList() ?? new List<PromotionProduct>();
            _context.PromotionProducts.RemoveRange(existingProducts);
            promotion.PromotionProducts = dto.Products?.Select(p => new PromotionProduct
            {
                ProductId = p.ProductId,
                DiscountPercent = p.DiscountPercent,
                PromotionId = id
            }).ToList();

            // cập nhật danh sách store
            var previousStoreIdsForRemoval = promotion.PromotionStores?.Select(ps => ps.StoreId).ToList() ?? new List<int>();
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
            var currentStoreIds = promotion.PromotionStores?.Select(ps => ps.StoreId).Distinct().ToList() ?? new List<int>();
            var affectedStoreIds = previousStoreIds.Union(currentStoreIds).Distinct().ToList();
            var removedStoreIds = previousStoreIdsForRemoval.Except(currentStoreIds).Distinct().ToList();

            if (promotion.StatusId == 1)
            {
                await ValidateStorePromotionAvailabilityAsync(currentStoreIds, promotion.Id);
                // active -> apply discount
                await ApplyDiscountToProductsAsync(promotion, currentStoreIds);

                if (removedStoreIds.Any())
                {
                    await RestoreOriginalPricesAsync(promotion, removedStoreIds, existingProductsSnapshot);
                }

                var msg = $"🎉 Chương trình khuyến mãi đã bắt đầu!\n\n" +
                    $"📅 Thời gian: {promotion.StartDate:dd/MM/yyyy} - {promotion.EndDate:dd/MM/yyyy}\n\n" +
                    $"🛍️ Sản phẩm áp dụng:\n{productsSummary}";
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
                if (affectedStoreIds.Any())
                {
                    await RestoreOriginalPricesAsync(promotion, affectedStoreIds, existingProductsSnapshot.Any() ? existingProductsSnapshot : null);
                }

                if (now > promotion.EndDate)
                {
                    // vừa kết thúc -> restore prices, notify end
                    var msg = $"⏰ Chương trình khuyến mãi đã kết thúc vào {promotion.EndDate:dd/MM/yyyy}.";
                    if (!string.IsNullOrWhiteSpace(productsSummary))
                        msg += $"\n\n🛍️ Sản phẩm đã áp dụng:\n{productsSummary}";
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
                    var msg = $"📢 Chương trình khuyến mãi sắp tới!\n\n" +
                        $"📅 Thời gian: {promotion.StartDate:dd/MM/yyyy} - {promotion.EndDate:dd/MM/yyyy}\n" +
                        $"⏰ {dayText}\n\n" +
                        $"🛍️ Sản phẩm áp dụng:\n{productsSummary}";
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
