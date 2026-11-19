
using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.DTOs
{
    public class UpdateCartItemDto
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
