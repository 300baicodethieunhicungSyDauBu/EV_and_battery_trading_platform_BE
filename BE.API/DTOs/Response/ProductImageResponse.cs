namespace BE.API.DTOs.Response
{
    public class ProductImageResponse
    {
        public int ImageId { get; set; }

        public int? ProductId { get; set; }

        public string ImageData { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }
    }
}
