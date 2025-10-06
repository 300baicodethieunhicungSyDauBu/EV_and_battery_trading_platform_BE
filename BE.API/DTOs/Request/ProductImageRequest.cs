namespace BE.API.DTOs.Request
{
    public class ProductImageRequest
    {
        public int? ProductId { get; set; }
        public IFormFile ImageFile { get; set; }
    }

}
