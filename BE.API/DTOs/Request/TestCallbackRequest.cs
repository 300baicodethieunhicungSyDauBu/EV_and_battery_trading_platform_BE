using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class TestCallbackRequest
    {
        [Required]
        public int PaymentId { get; set; }
    }
}
