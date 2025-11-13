using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Receipt;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly ShoeStoreDbContext _context;
        private const long DefaultCreatedBy = 1; // theo bạn nói

        public ReceiptService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReceiptDto>> GetAllAsync()
        {
            var receipts = await _context.Receipts
                .Include(r => r.Supplier)
                .Include(r => r.Store)
                .Include(r => r.Status)
                .Include(r => r.Creator)
                .Include(r => r.ReceiptDetails!)!.ThenInclude(rd => rd.Product)
                .AsNoTracking()
                .ToListAsync();

            return receipts.Select(r => new ReceiptDto
            {
                Id = r.Id,
                ReceiptNumber = r.ReceiptNumber,
                SupplierId = r.SupplierId,
                SupplierName = r.Supplier?.Name ?? string.Empty,
                StoreId = r.StoreId,
                StoreName = r.Store?.Name ?? string.Empty,
                CreatedBy = r.CreatedBy,
                CreatorName = r.Creator?.FullName ?? "System",
                CreatedAt = r.CreatedAt,
                StatusId = r.StatusId,
                // ✅ lấy TotalAmount từ DB (đã tính sẵn)
                TotalAmount = r.TotalAmount,
                Details = r.ReceiptDetails?.Select(d => new ReceiptDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? string.Empty,
                    SKU = d.Product?.SKU,
                    QuantityOrdered = d.QuantityOrdered,
                    ReceivedQuantity = d.ReceivedQuantity,
                    UnitPrice = d.UnitPrice
                }).ToList() ?? new List<ReceiptDetailDto>()
            }).ToList();
        }

        public async Task<ReceiptDto?> GetByIdAsync(long id)
        {
            var r = await _context.Receipts
                .Include(r => r.Supplier)
                .Include(r => r.Store)
                .Include(r => r.Status)
                .Include(r => r.Creator)
                .Include(r => r.ReceiptDetails!)!.ThenInclude(rd => rd.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return null;

            return new ReceiptDto
            {
                Id = r.Id,
                ReceiptNumber = r.ReceiptNumber,
                SupplierId = r.SupplierId,
                SupplierName = r.Supplier?.Name ?? string.Empty,
                StoreId = r.StoreId,
                StoreName = r.Store?.Name ?? string.Empty,
                CreatedBy = r.CreatedBy,
                CreatorName = r.Creator?.FullName ?? "System",
                CreatedAt = r.CreatedAt,
                StatusId = r.StatusId,
                TotalAmount = r.TotalAmount,
                Details = r.ReceiptDetails?.Select(d => new ReceiptDetailDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? string.Empty,
                    SKU = d.Product?.SKU,
                    QuantityOrdered = d.QuantityOrdered,
                    ReceivedQuantity = d.ReceivedQuantity,
                    UnitPrice = d.UnitPrice
                }).ToList() ?? new List<ReceiptDetailDto>()
            };
        }


        public async Task<ReceiptDto> CreateAsync(CreateReceiptDto dto)
        {
            // validate supplier exists
            var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null) throw new InvalidOperationException("Supplier not found.");

            // prepare receipt
            var now = DateTime.UtcNow;
            var receipt = new Receipt
            {
                ReceiptNumber = $"RCPT-{now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}",
                SupplierId = dto.SupplierId,
                CreatedBy = DefaultCreatedBy,
                StoreId = dto.StoreId,
                StatusId = 1, // theo yêu cầu
                CreatedAt = now,
                TotalAmount = 0m,
                ReceiptDetails = new List<ReceiptDetail>()
            };

            // Add details
            if (dto.Details != null && dto.Details.Any())
            {
                foreach (var d in dto.Details)
                {
                    var product = await _context.Products.FindAsync(d.ProductId);
                    if (product == null) throw new InvalidOperationException($"Product with id {d.ProductId} not found.");

                    var unitPrice = product.CostPrice;
                    var detail = new ReceiptDetail
                    {
                        ProductId = d.ProductId,
                        QuantityOrdered = d.QuantityOrdered,
                        ReceivedQuantity = null,
                        UnitPrice = unitPrice
                    };

                    receipt.ReceiptDetails.Add(detail);

                    receipt.TotalAmount += unitPrice * d.QuantityOrdered;
                }
            }

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(receipt.Id) ?? throw new InvalidOperationException("Failed to return created receipt.");
        }

        public async Task<ReceiptDto?> UpdateAsync(long id, UpdateReceiptDto dto)
        {
            var receipt = await _context.Receipts
                .Include(r => r.ReceiptDetails)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return null;

            // don't allow updating received qty here (this is pre-receipt edit)
            receipt.SupplierId = dto.SupplierId;
            receipt.StoreId = dto.StoreId;

            // Replace details with new ordered quantities.
            // NOTE: This will clear ReceivedQuantity information; since this update is only pre-receipt as you requested,
            // we assume ReceivedQuantity were null. If you want to preserve ReceivedQuantity if present, adjust logic.
            _context.ReceiptDetails.RemoveRange(receipt.ReceiptDetails ?? Enumerable.Empty<ReceiptDetail>());

            receipt.ReceiptDetails = new List<ReceiptDetail>();
            receipt.TotalAmount = 0m;
            if (dto.Details != null && dto.Details.Any())
            {
                foreach (var d in dto.Details)
                {
                    var product = await _context.Products.FindAsync(d.ProductId);
                    if (product == null) throw new InvalidOperationException($"Product with id {d.ProductId} not found.");
                    var unitPrice = product.CostPrice;

                    var rd = new ReceiptDetail
                    {
                        ProductId = d.ProductId,
                        QuantityOrdered = d.QuantityOrdered,
                        ReceivedQuantity = null,
                        UnitPrice = unitPrice
                    };
                    receipt.ReceiptDetails.Add(rd);
                    receipt.TotalAmount += unitPrice * d.QuantityOrdered;
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<ReceiptDto?> UpdateReceivedAsync(long id, UpdateReceiptReceivedDto dto)
        {
            var receipt = await _context.Receipts
                .Include(r => r.ReceiptDetails)
                .Include(r => r.Store)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null) return null;
            if (receipt.StatusId != 1)
                throw new InvalidOperationException("Receipt đã được xác nhận trước đó và không thể cập nhật lại.");

            if (receipt.StoreId == null)
                throw new InvalidOperationException("Receipt.StoreId is required to update received quantities (to know which store to increase).");

            // Build a map of detail by id for quick lookup
            var detailsById = (receipt.ReceiptDetails ?? Enumerable.Empty<ReceiptDetail>())
                .ToDictionary(d => d.Id);

            // Update received quantities
            foreach (var upd in dto.Details)
            {
                if (!detailsById.TryGetValue(upd.ReceiptDetailId, out var detail))
                    throw new InvalidOperationException($"ReceiptDetail id {upd.ReceiptDetailId} not found in receipt {id}.");

                // set received quantity
                detail.ReceivedQuantity = upd.ReceivedQuantity;
            }

            // Recalculate total amount based on received quantities
            decimal total = 0m;
            foreach (var detail in receipt.ReceiptDetails ?? Enumerable.Empty<ReceiptDetail>())
            {
                var recv = detail.ReceivedQuantity ?? 0;
                total += recv * detail.UnitPrice;
            }

            receipt.TotalAmount = total;

            // Update storeproduct: add received qty into corresponding StoreProduct
            var storeId = receipt.StoreId!.Value;
            var productIds = (receipt.ReceiptDetails ?? Enumerable.Empty<ReceiptDetail>())
                .Select(rd => rd.ProductId)
                .Distinct()
                .ToList();

            var productMap = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var detail in receipt.ReceiptDetails ?? Enumerable.Empty<ReceiptDetail>())
            {
                var recv = detail.ReceivedQuantity ?? 0;
                if (recv <= 0) continue;

                var sp = await _context.StoreProducts
                    .FirstOrDefaultAsync(x => x.ProductId == detail.ProductId && x.StoreId == storeId);

                if (sp == null)
                {
                    productMap.TryGetValue(detail.ProductId, out var product);
                    sp = new StoreProduct
                    {
                        ProductId = detail.ProductId,
                        StoreId = storeId,
                        Quantity = recv,
                        SalePrice = product?.OriginalPrice ?? 0m
                    };
                    _context.StoreProducts.Add(sp);
                }
                else
                {
                    sp.Quantity += recv;
                    if (sp.SalePrice <= 0 && productMap.TryGetValue(detail.ProductId, out var productForUpdate))
                    {
                        sp.SalePrice = productForUpdate.OriginalPrice;
                    }
                }
            }

            // Finally mark receipt as received/completed: StatusId = 2
            receipt.StatusId = 2;
            receipt.ReceivedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var r = await _context.Receipts
                .Include(x => x.ReceiptDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return false;

            _context.ReceiptDetails.RemoveRange(r.ReceiptDetails ?? Enumerable.Empty<ReceiptDetail>());
            _context.Receipts.Remove(r);

            await _context.SaveChangesAsync();
            return true;
        }

        // helper mapping

    }
}
