namespace BE.API.DTOs.Request
{
    public class ProductImageRequest
    {
        public int? ProductId { get; set; }
        public string? Name { get; set; }
        public IFormFile ImageFile { get; set; } = null!;
    }

}
