using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Comment;
using ShoeStore.Application.Interfaces;
using ShoeStore.Infrastructure.Persistence;
using ShoeStore.Domain.Entities;

namespace ShoeStore.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ShoeStoreDbContext _context;

        public CommentService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentDto>> GetAllAsync()
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Content = c.Content,
                    Rating = c.Rating,
                    CreatedAt = c.CreatedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommentDto>> GetByProductIdAsync(int productId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.FullName,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Content = c.Content,
                    Rating = c.Rating,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CommentDto?> GetByIdAsync(long id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return null;

            return new CommentDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = comment.User.FullName,
                ProductId = comment.ProductId,
                ProductName = comment.Product.Name,
                Content = comment.Content,
                Rating = comment.Rating,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<CommentDto> CreateAsync(CreateCommentDto dto)
        {
            var comment = new Comment
            {
                UserId = dto.UserId,
                ProductId = dto.ProductId,
                Content = dto.Content,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserId);
            var product = await _context.Products.FindAsync(dto.ProductId);

            return new CommentDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = user?.FullName ?? "",
                ProductId = comment.ProductId,
                ProductName = product?.Name ?? "",
                Content = comment.Content,
                Rating = comment.Rating,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<CommentDto?> UpdateAsync(long id, UpdateCommentDto dto)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return null;

            comment.Content = dto.Content;
            comment.Rating = dto.Rating;

            await _context.SaveChangesAsync();

            return new CommentDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = comment.User.FullName,
                ProductId = comment.ProductId,
                ProductName = comment.Product.Name,
                Content = comment.Content,
                Rating = comment.Rating,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
