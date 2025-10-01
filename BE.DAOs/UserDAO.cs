using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class UserDAO
    {
        private static UserDAO instance;
        private static EvandBatteryTradingPlatformContext dbcontext;
        private UserDAO()
        {
            dbcontext = new EvandBatteryTradingPlatformContext();
        }

        public static UserDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserDAO();
                }
                return instance;
            }
        }

        public User GetAccountByEmailAndPassword(string email, string password)
        {
            return dbcontext.Users.FirstOrDefault(a => a.Email == email && a.PasswordHash == password);
        }
    }
}
