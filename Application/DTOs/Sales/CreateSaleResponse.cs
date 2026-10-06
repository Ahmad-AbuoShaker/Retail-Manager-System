namespace Application.DTOs.Sales;

public class CreateSaleResponse
{
    public int SaleId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime SaleDate { get; set; }
}