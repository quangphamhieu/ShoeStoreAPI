using Microsoft.EntityFrameworkCore;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence.Interceptors;
using ShoeStore.Infrastructure.Security;

namespace ShoeStore.Infrastructure.Persistence
{
    public class ShoeStoreDbContext : DbContext
    {
        private readonly AuditInterceptor _auditInterceptor;
        public ShoeStoreDbContext(DbContextOptions<ShoeStoreDbContext> options, AuditInterceptor auditInterceptor) : base(options)
        {
            _auditInterceptor = auditInterceptor;
        }

        // DbSets
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Receipt> Receipts { get; set; } = null!;
        public DbSet<ReceiptDetail> ReceiptDetails { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;
        public DbSet<PromotionProduct> PromotionProducts { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<StoreProduct> StoreProducts { get; set; } = null!;
        public DbSet<PromotionStore> PromotionStores { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Status ----------
            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Statuses");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Code).IsRequired().HasMaxLength(50);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Description).HasMaxLength(1000);
                entity.HasIndex(s => s.Code).IsUnique(false); // nếu bạn muốn unique thì IsUnique(true)
            });

            // ---------- Role ----------
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Code).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            });

            // ---------- Store ----------
            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Stores");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Code).HasMaxLength(50);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(250);
                entity.Property(s => s.Address).HasMaxLength(500);
                entity.Property(s => s.Phone).HasMaxLength(50);
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(s => s.Status)
                      .WithMany(st => st.Stores)
                      .HasForeignKey(s => s.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.Users)
                      .WithOne(u => u.Store)
                      .HasForeignKey(u => u.StoreId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(s => s.Orders)
                      .WithOne(o => o.Store)
                      .HasForeignKey(o => o.StoreId)
                      .OnDelete(DeleteBehavior.SetNull);

                // ✅ New many-to-many via StoreProduct
                entity.HasMany(s => s.StoreProducts)
                      .WithOne(sp => sp.Store)
                      .HasForeignKey(sp => sp.StoreId);

                entity.HasMany(s => s.PromotionStores)
                      .WithOne(ps => ps.Store)
                      .HasForeignKey(ps => ps.StoreId);
            });


            modelBuilder.Entity<StoreProduct>(entity =>
            {
                entity.ToTable("StoreProducts");
                entity.HasKey(sp => new { sp.StoreId, sp.ProductId });

                entity.Property(sp => sp.Quantity).IsRequired();
                entity.Property(sp => sp.SalePrice).HasColumnType("decimal(18,2)");

                entity.HasOne(sp => sp.Store)
                      .WithMany(s => s.StoreProducts)
                      .HasForeignKey(sp => sp.StoreId);

                entity.HasOne(sp => sp.Product)
                      .WithMany(p => p.StoreProducts)
                      .HasForeignKey(sp => sp.ProductId);
            });

            // ---------- Supplier ----------
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Code).HasMaxLength(50);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(250);
                entity.Property(s => s.ContactInfo).HasMaxLength(1000);

                // Supplier.Status
                entity.HasOne(s => s.Status)
                      .WithMany(st => st.Suppliers)
                      .HasForeignKey(s => s.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Products relation (Supplier may have many Products)
                entity.HasMany(s => s.Products)
                      .WithOne(p => p.Supplier)
                      .HasForeignKey(p => p.SupplierId)
                      .OnDelete(DeleteBehavior.SetNull); // supplier nullable on Product
            });

            // ---------- User ----------
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(250);
                entity.Property(u => u.Phone).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).HasMaxLength(250);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Store)
                      .WithMany(s => s.Users)
                      .HasForeignKey(u => u.StoreId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(u => u.Status)
                      .WithMany(st => st.Users)
                      .HasForeignKey(u => u.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(u => u.Phone).IsUnique(false); // nếu phone phải unique => IsUnique(true)
            });

            // ---------- Brand ----------
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("Brands");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Code).HasMaxLength(50);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(250);
                entity.Property(b => b.Description).HasMaxLength(1000);

                entity.HasOne(b => b.Status)
                      .WithMany(st => st.Brands)
                      .HasForeignKey(b => b.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(b => b.Products)
                      .WithOne(p => p.Brand)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ---------- Product ----------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.SKU).HasMaxLength(500);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(500);
                entity.Property(p => p.Color).HasMaxLength(100);
                entity.Property(p => p.Size).HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(2000);
                entity.Property(p => p.ImageUrl).HasMaxLength(1000);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Prices
                entity.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.OriginalPrice).HasColumnType("decimal(18,2)");

                entity.HasIndex(p => p.SKU).IsUnique(); // SKU unique

                // Relations
                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Supplier)
                      .WithMany(s => s.Products)
                      .HasForeignKey(p => p.SupplierId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Status)
                      .WithMany(st => st.Products)
                      .HasForeignKey(p => p.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ New many-to-many via StoreProduct
                entity.HasMany(p => p.StoreProducts)
                      .WithOne(sp => sp.Product)
                      .HasForeignKey(sp => sp.ProductId);
            });


            // ---------- Receipt ----------
            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.ToTable("Receipts");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.ReceiptNumber).IsRequired().HasMaxLength(100);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(r => r.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                entity.Property(r => r.ReceivedDate).HasColumnType("datetime2").IsRequired(false);

                entity.HasIndex(r => r.ReceiptNumber).IsUnique();

                entity.HasOne(r => r.Supplier)
                      .WithMany() // nếu Supplier chưa có navigation
                      .HasForeignKey(r => r.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Creator)
                      .WithMany() // nếu User chưa có navigation
                      .HasForeignKey(r => r.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Store)
                      .WithMany()
                      .HasForeignKey(r => r.StoreId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Status)
                      .WithMany(st => st.Receipts)
                      .HasForeignKey(r => r.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(r => r.ReceiptDetails)
                      .WithOne(rd => rd.Receipt)
                      .HasForeignKey(rd => rd.ReceiptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- ReceiptDetail ----------
            modelBuilder.Entity<ReceiptDetail>(entity =>
            {
                entity.ToTable("ReceiptDetails");
                entity.HasKey(rd => rd.Id);

                entity.Property(rd => rd.QuantityOrdered).IsRequired();
                entity.Property(rd => rd.ReceivedQuantity).IsRequired(false);
                entity.Property(rd => rd.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();

                entity.HasOne(rd => rd.Product)
                      .WithMany()
                      .HasForeignKey(rd => rd.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Promotion ----------
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotions");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Code).HasMaxLength(100);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(300);
                entity.Property(p => p.StartDate).IsRequired();
                entity.Property(p => p.EndDate).IsRequired();

                entity.HasOne(p => p.Status)
                      .WithMany(st => st.Promotions)
                      .HasForeignKey(p => p.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.PromotionProducts)
                      .WithOne(pp => pp.Promotion)
                      .HasForeignKey(pp => pp.PromotionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.PromotionStores)
                      .WithOne(ps => ps.Promotion)
                      .HasForeignKey(ps => ps.PromotionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PromotionStore>(entity =>
            {
                entity.ToTable("PromotionStores");
                entity.HasKey(ps => new { ps.PromotionId, ps.StoreId });

                entity.HasOne(ps => ps.Promotion)
                      .WithMany(p => p.PromotionStores)
                      .HasForeignKey(ps => ps.PromotionId);

                entity.HasOne(ps => ps.Store)
                      .WithMany(s => s.PromotionStores)
                      .HasForeignKey(ps => ps.StoreId);
            });

            // ---------- PromotionProduct ----------
            modelBuilder.Entity<PromotionProduct>(entity =>
            {
                entity.ToTable("PromotionProducts");
                entity.HasKey(pp => pp.Id);
                entity.Property(pp => pp.DiscountPercent).HasColumnType("decimal(5,2)");
                entity.HasOne(pp => pp.Product)
                      .WithMany()
                      .HasForeignKey(pp => pp.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Order ----------
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(100);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.UpdatedAt).IsRequired(false);

                // Customer (required)
                entity.HasOne(o => o.Customer)
                      .WithMany()
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Creator (optional)
                entity.HasOne(o => o.Creator)
                      .WithMany()
                      .HasForeignKey(o => o.CreatedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(o => o.Store)
                      .WithMany(s => s.Orders)
                      .HasForeignKey(o => o.StoreId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(o => o.Status)
                      .WithMany(st => st.Orders)
                      .HasForeignKey(o => o.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.OrderDetails)
                      .WithOne(od => od.Order)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // unique index on OrderNumber if desired:
                entity.HasIndex(o => o.OrderNumber).IsUnique(false);
            });

            // ---------- OrderDetail ----------
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasKey(od => od.Id);

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId);

                entity.HasOne(od => od.Product)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(od => od.ProductId);
            });


            // ---------- Cart ----------
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Status)
                      .WithMany() // Status class did not declare Cart navigation
                      .HasForeignKey(c => c.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.CartItems)
                      .WithOne(ci => ci.Cart)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- CartItem ----------
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(ci => ci.Id);
                entity.Property(ci => ci.Quantity).IsRequired();
                entity.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(ci => ci.Product)
                      .WithMany()
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Comment ----------
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).IsRequired().HasMaxLength(2000);
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Notification ----------
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Code).HasMaxLength(100);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(300);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(4000);
                entity.Property(n => n.Type).HasMaxLength(100);
                entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ---------- AuditLog ----------
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(200);
                entity.Property(a => a.TableName).IsRequired().HasMaxLength(200);
                entity.Property(a => a.OldValue).HasMaxLength(4000);
                entity.Property(a => a.NewValue).HasMaxLength(4000);
                entity.Property(a => a.Description).HasMaxLength(2000);
                entity.Property(a => a.IPAddress).HasMaxLength(100);
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Code = "ACTIVE", Name = "Active", Description = "Active status" },
                new Status { Id = 2, Code = "INACTIVE", Name = "Inactive", Description = "Inactive status" },
                new Status { Id = 3, Code = "PAYMENT_SUCCESS", Name = "Thanh toán thành công", Description = "Đơn hàng đã thanh toán thành công" },
                new Status { Id = 4, Code = "PENDING_CONFIRMATION", Name = "Chờ xác nhận", Description = "Đơn hàng đang chờ xác nhận" },
                new Status { Id = 5, Code = "CONFIRMED", Name = "Xác nhận", Description = "Đơn hàng đã được xác nhận" },
                new Status { Id = 6, Code = "CANCELLED", Name = "Đã hủy", Description = "Đơn hàng đã bị hủy" }
            );
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Code = "SUPER ADMIN", Name = "Super Admin" },
                new Role { Id = 2, Code = "ADMIN", Name = "Adminitrator" },
                new Role { Id = 3, Code = "STAFF", Name = "Staff" },
                new Role { Id = 4, Code = "CUSTOMER", Name = "Customer" }
            );
            var passwordHelper = new PasswordHelper();
            var hasherPass = passwordHelper.HashPassword("123456");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Admin",
                Phone = "0345602265",
                Email = "admin@gmail.com",
                PasswordHash = hasherPass,
                RoleId = 1,
                StatusId = 1,
                CreatedAt = DateTime.Now,
            });
        }
    }
}
