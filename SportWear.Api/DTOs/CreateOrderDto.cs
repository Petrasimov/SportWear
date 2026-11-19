
using System.ComponentModel.DataAnnotations;

namespace SportWear.Api.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;
    }
}
