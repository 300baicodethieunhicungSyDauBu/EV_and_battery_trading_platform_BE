using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request;

public class MessageRequest
{
    [Required]
    public int ChatId { get; set; }

    [Required]
    public int SenderId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; set; } = null!;
}
