using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteRepo _favoriteRepo;

        public FavoriteController(IFavoriteRepo favoriteRepo)
        {
            _favoriteRepo = favoriteRepo;
        }

        [HttpGet]
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

        [HttpPost]
        public ActionResult CreateFavorite([FromBody] FavoriteRequest request)
        {
            try
            {
                var favorite = new Favorite
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId
                };

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

        [HttpDelete("{id}")]
        public ActionResult DeleteFavorite(int id)
        {
            try
            {
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

        [HttpGet("user/{userId}")]
        public ActionResult GetFavoritesByUserId(int userId)
        {
            try
            {
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
