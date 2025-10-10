namespace ShoeStore.Domain.Entities;

public class Promotion
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? StoreId { get; set; }
    public int StatusId { get; set; }

    public Store? Store { get; set; }
    public Status Status { get; set; } = null!;
    public ICollection<PromotionProduct>? PromotionProducts { get; set; }
}


