namespace Application.DTOs.Sales;

public class SalesReportDto
{
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string CashierFirstName { get; set; } = string.Empty;
    public string CashierLastName { get; set; } = string.Empty;
    public string CashierEmail { get; set; } = string.Empty;
}