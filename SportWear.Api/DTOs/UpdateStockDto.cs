
using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.DTOs
{
    public class UpdateStockDto
    {
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}
