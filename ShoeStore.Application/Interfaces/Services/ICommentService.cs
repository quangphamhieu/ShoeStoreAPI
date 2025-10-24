using ShoeStore.Application.Dtos.Comment;

namespace ShoeStore.Application.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetAllAsync();
        Task<IEnumerable<CommentDto>> GetByProductIdAsync(int productId);
        Task<CommentDto?> GetByIdAsync(long id);
        Task<CommentDto> CreateAsync(CreateCommentDto dto);
        Task<CommentDto?> UpdateAsync(long id, UpdateCommentDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
