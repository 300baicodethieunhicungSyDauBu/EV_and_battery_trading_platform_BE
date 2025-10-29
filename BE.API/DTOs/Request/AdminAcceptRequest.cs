using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class AdminAcceptRequest
    {
        [Required]
        public int ProductId { get; set; }
    }

    public class AdminAcceptWrapperRequest
    {
        [Required]
        public AdminAcceptRequest Request { get; set; } = new AdminAcceptRequest();
    }
}
