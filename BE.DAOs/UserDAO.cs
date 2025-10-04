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
            var user = dbcontext.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public User Register(User user)
        {
            user.CreatedDate = DateTime.Now;
            user.AccountStatus = "Active";
            dbcontext.Users.Add(user);
            dbcontext.SaveChanges();
            return user;
        }

        public List<User> GetAllUsers()
        {
            return dbcontext.Users.Include(u => u.Role).ToList();
        }

        public User GetUserById(int userId)
        {
            return dbcontext.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == userId);
        }

        // Update User
        public User UpdateUser(User updatedUser)
        {
            try
            {
                var existingUser = dbcontext.Users.Find(updatedUser.UserId);
                if (existingUser == null)
                {
                    throw new Exception("User not found");
                }

                // Update properties
                existingUser.Email = updatedUser.Email;
                existingUser.FullName = updatedUser.FullName;
                existingUser.Phone = updatedUser.Phone;
                existingUser.Avatar = updatedUser.Avatar;

                // Only update password if it's provided
                if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
                {
                    existingUser.PasswordHash = updatedUser.PasswordHash;
                }

                dbcontext.Users.Update(existingUser);
                dbcontext.SaveChanges();

                return existingUser;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user: " + ex.Message);
            }
        }

        // Delete User
        public bool DeleteUser(int userId)
        {
            try
            {
                var user = dbcontext.Users.Find(userId);
                if (user == null)
                {
                    return false;
                }

                dbcontext.Users.Remove(user);
                dbcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user: " + ex.Message);
            }
        }
    }
}
