using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteRepo _favoriteRepo;

        public FavoriteController(IFavoriteRepo favoriteRepo)
        {
            _favoriteRepo = favoriteRepo;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<FavoriteResponse>> GetAllFavorites()
        {
            try
            {
                var favorites = _favoriteRepo.GetAllFavorites();
                var response = favorites.Select(f => new
                {
                    f.FavoriteId,
                    f.UserId,
                    f.ProductId,
                    f.CreatedDate,
                    ProductName = f.Product?.Title,
                    UserName = f.User?.FullName
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult GetFavoriteById(int id)
        {
            try
            {
                var favorite = _favoriteRepo.GetFavoriteById(id);
                if (favorite == null)
                {
                    return NotFound();
                }

                var response = new
                {
                    favorite.FavoriteId,
                    favorite.UserId,
                    favorite.ProductId,
                    favorite.CreatedDate,
                    ProductName = favorite.Product?.Title,
                    UserName = favorite.User?.FullName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // ❤️ THÊM SẢN PHẨM VÀO YÊU THÍCH (Member only)
        // Input: { userId, productId }
        // Output: Favorite info
        [HttpPost]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult CreateFavorite([FromBody] FavoriteRequest request)
        {
            try
            {
                // 1️⃣ Tạo favorite mới
                var favorite = new Favorite
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId
                };

                // 2️⃣ Lưu vào database
                var createdFavorite = _favoriteRepo.CreateFavorite(favorite);

                var response = new
                {
                    createdFavorite.FavoriteId,
                    createdFavorite.UserId,
                    createdFavorite.ProductId,
                    createdFavorite.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult UpdateFavorite(int id, [FromBody] FavoriteRequest request)
        {
            try
            {
                var existingFavorite = _favoriteRepo.GetFavoriteById(id);
                if (existingFavorite == null)
                {
                    return NotFound("Favorite not found");
                }

                // Verify ownership
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (existingFavorite.UserId != userId)
                {
                    return Forbid("You can only update your own favorites");
                }

                existingFavorite.UserId = request.UserId;
                existingFavorite.ProductId = request.ProductId;

                var updatedFavorite = _favoriteRepo.UpdateFavorite(existingFavorite);

                var response = new
                {
                    updatedFavorite.FavoriteId,
                    updatedFavorite.UserId,
                    updatedFavorite.ProductId,
                    updatedFavorite.CreatedDate,
                    Message = "Favorite updated successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // 🗑️ XÓA SẢN PHẨM KHỎI YÊU THÍCH (Member only)
        // Input: favoriteId
        // Output: Success/NotFound
        [HttpDelete("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult DeleteFavorite(int id)
        {
            try
            {
                // 1️⃣ Xóa favorite
                var result = _favoriteRepo.DeleteFavorite(id);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // 📋 XEM DANH SÁCH YÊU THÍCH CỦA USER (Member only)
        // Input: userId
        // Output: Danh sách favorites của user đó
        // Auth: Chỉ xem được favorites của chính mình (trừ admin)
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult GetFavoritesByUserId(int userId)
        {
            try
            {
                // 1️⃣ Kiểm tra quyền (chỉ xem được favorites của mình)
                var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var isAdmin = User.IsInRole("1");
                
                if (!isAdmin && currentUserId != userId)
                {
                    return Forbid("You can only access your own favorites");
                }

                // 2️⃣ Lấy danh sách favorites
                var favorites = _favoriteRepo.GetFavoritesByUserId(userId);
                var response = favorites.Select(f => new
                {
                    f.FavoriteId,
                    f.UserId,
                    f.ProductId,
                    f.CreatedDate,
                    ProductName = f.Product?.Title,
                    UserName = f.User?.FullName
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
