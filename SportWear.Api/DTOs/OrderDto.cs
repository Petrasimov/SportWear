
namespace SportWear.Api.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}
