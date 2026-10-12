namespace Application.DTOs.Inventory;

public class CreateInventoryRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTime? PurchaseDate { get; set; }
}