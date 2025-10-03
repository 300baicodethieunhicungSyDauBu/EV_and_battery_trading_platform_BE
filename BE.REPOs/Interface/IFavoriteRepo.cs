using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IFavoriteRepo
    {
        List<Favorite> GetAllFavorites();
        Favorite GetFavoriteById(int id);
        Favorite CreateFavorite(Favorite favorite);
        Favorite UpdateFavorite(Favorite favorite);
        bool DeleteFavorite(int id);
        List<Favorite> GetFavoritesByUserId(int userId);
    }
}
