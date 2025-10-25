using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class SellerConfirmRequest
    {
        [Required]
        public int ProductId { get; set; }
    }

    public class SellerConfirmWrapperRequest
    {
        [Required]
        public SellerConfirmRequest Request { get; set; } = new SellerConfirmRequest();
    }
}
