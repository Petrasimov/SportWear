
using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.DTOs
{
    public class AddToCartDto
    {
        [Range(1, int.MaxValue)]
        public int ProductSizeId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
