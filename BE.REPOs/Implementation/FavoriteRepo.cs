using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Implementation
{
    public class FavoriteRepo : IFavoriteRepo
    {
        public List<Favorite> GetAllFavorites()
        {
            return FavoriteDAO.Instance.GetAllFavorites();
        }

        public Favorite GetFavoriteById(int id)
        {
            return FavoriteDAO.Instance.GetFavoriteById(id);
        }

        public Favorite CreateFavorite(Favorite favorite)
        {
            return FavoriteDAO.Instance.CreateFavorite(favorite);
        }

        public Favorite UpdateFavorite(Favorite favorite)
        {
            return FavoriteDAO.Instance.UpdateFavorite(favorite);
        }

        public bool DeleteFavorite(int id)
        {
            return FavoriteDAO.Instance.DeleteFavorite(id);
        }

        public List<Favorite> GetFavoritesByUserId(int userId)
        {
            return FavoriteDAO.Instance.GetFavoritesByUserId(userId);
        }
    }
}
