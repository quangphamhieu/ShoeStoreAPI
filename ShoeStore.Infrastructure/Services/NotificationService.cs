using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Notification;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ShoeStoreDbContext _context;

        public NotificationService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        public async Task<IEnumerable<NotificationDto>> GetAllAsync()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Code = n.Code,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt
            });
        }

        // ✅ GET BY ID
        public async Task<NotificationDto?> GetByIdAsync(long id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return null;

            return new NotificationDto
            {
                Id = n.Id,
                Code = n.Code,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt
            };
        }

        // ✅ CREATE
        public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
        {
            var now = DateTime.UtcNow;
            var code = $"NTF-{now:yyyyMMddHHmmss}";

            var entity = new Notification
            {
                Code = code,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                CreatedAt = now
            };

            _context.Notifications.Add(entity);
            await _context.SaveChangesAsync();

            return new NotificationDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Title = entity.Title,
                Message = entity.Message,
                Type = entity.Type,
                CreatedAt = entity.CreatedAt
            };
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(long id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
