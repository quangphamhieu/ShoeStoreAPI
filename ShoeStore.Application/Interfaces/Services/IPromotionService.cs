using ShoeStore.Application.Dtos.Promotion;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetAllAsync();
        Task<PromotionDto?> GetByIdAsync(int id);
        Task<PromotionDto> CreateAsync(CreatePromotionDto dto);
        Task<PromotionDto?> UpdateAsync(int id, UpdatePromotionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
