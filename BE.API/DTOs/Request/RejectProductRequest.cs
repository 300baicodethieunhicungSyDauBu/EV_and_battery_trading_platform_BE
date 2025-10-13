using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class RejectProductRequest
    {
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? RejectionReason { get; set; }
    }
}
