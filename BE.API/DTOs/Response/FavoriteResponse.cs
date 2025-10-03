namespace BE.API.DTOs.Response
{
    public class FavoriteResponse
    {
        public int FavoriteId { get; set; }

        public int? UserId { get; set; }

        public int? ProductId { get; set; }

        public DateTime? CreatedDate { get; set; }

    }
}
