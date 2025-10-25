using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class TestUpdateStatusRequest
    {
        [Required]
        public string NewStatus { get; set; } = string.Empty;
    }
}
