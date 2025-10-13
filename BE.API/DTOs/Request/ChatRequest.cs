using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request;

public class ChatRequest
{
    [Required]
    public int User1Id { get; set; }

    [Required]
    public int User2Id { get; set; }
}
