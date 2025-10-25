using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class TestSellerConfirmRequest
    {
        [Required]
        public int SellerId { get; set; }
        
        [Required]
        public string NewStatus { get; set; } = string.Empty;
    }
}
