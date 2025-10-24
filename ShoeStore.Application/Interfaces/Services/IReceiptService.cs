using ShoeStore.Application.Dtos.Receipt;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IReceiptService
    {
        Task<IEnumerable<ReceiptDto>> GetAllAsync();
        Task<ReceiptDto?> GetByIdAsync(long id);

        // Create: CreatedBy default sẽ là 1
        Task<ReceiptDto> CreateAsync(CreateReceiptDto dto);

        // Update before receiving (change ordered quantities, supplier, store)
        Task<ReceiptDto?> UpdateAsync(long id, UpdateReceiptDto dto);

        // Update received quantities (finalize receive): will update StoreProduct qty and set StatusId = 2
        Task<ReceiptDto?> UpdateReceivedAsync(long id, UpdateReceiptReceivedDto dto);

        Task<bool> DeleteAsync(long id);
    }
}
