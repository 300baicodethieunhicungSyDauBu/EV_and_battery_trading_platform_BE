using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class FavoriteDAO
    {
        private static FavoriteDAO? instance;
        private static EvandBatteryTradingPlatformContext? dbcontext;

        private FavoriteDAO()
        {
            dbcontext = new EvandBatteryTradingPlatformContext();
        }

        public static FavoriteDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FavoriteDAO();
                }
                return instance;
            }
        }

        public List<Favorite> GetAllFavorites()
        {
            return dbcontext.Favorites
                .Include(f => f.Product)
                .Include(f => f.User)
                .ToList();
        }

        public Favorite? GetFavoriteById(int id)
        {
            return dbcontext.Favorites
                .Include(f => f.Product)
                .Include(f => f.User)
                .FirstOrDefault(f => f.FavoriteId == id);
        }

        public Favorite CreateFavorite(Favorite favorite)
        {
            favorite.CreatedDate = DateTime.Now;
            dbcontext.Favorites.Add(favorite);
            dbcontext.SaveChanges();
            return favorite;
        }

        public Favorite UpdateFavorite(Favorite favorite)
        {
            dbcontext.Favorites.Update(favorite);
            dbcontext.SaveChanges();
            return favorite;
        }

        public bool DeleteFavorite(int id)
        {
            var favorite = dbcontext.Favorites.Find(id);
            if (favorite == null) return false;

            dbcontext.Favorites.Remove(favorite);
            dbcontext.SaveChanges();
            return true;
        }

        public List<Favorite> GetFavoritesByUserId(int userId)
        {
            return dbcontext.Favorites
                .Include(f => f.Product)
                .Include(f => f.User)
                .Where(f => f.UserId == userId)
                .ToList();
        }
    }
}

