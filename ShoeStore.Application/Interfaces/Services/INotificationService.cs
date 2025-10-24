using ShoeStore.Application.Dtos.Notification;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllAsync();
        Task<NotificationDto?> GetByIdAsync(long id);
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
