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
        private static UserDAO? instance;
        public static EvandBatteryTradingPlatformContext? dbcontext;
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

        public User? GetUserById(int userId)
        {
            return dbcontext.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == userId);
        }

        public User? GetUserByEmail(string email)
        {
            return dbcontext.Users.Include(u => u.Role).FirstOrDefault(u => u.Email == email);
        }

        // OAuth methods
        public User? GetUserByOAuth(string provider, string oauthId)
        {
            return dbcontext.Users.Include(u => u.Role).FirstOrDefault(u => u.OAuthProvider == provider && u.OAuthId == oauthId);
        }

        public User CreateOAuthUser(string provider, string oauthId, string email, string fullName, string? avatar = null)
        {
            var user = new User
            {
                OAuthProvider = provider,
                OAuthId = oauthId,
                OAuthEmail = email,
                Email = email,
                FullName = fullName,
                Avatar = avatar,
                PasswordHash = "", // OAuth users don't need password
                AccountStatus = "Active",
                CreatedDate = DateTime.Now,
                RoleId = 2 // Default role for members
            };

            dbcontext.Users.Add(user);
            dbcontext.SaveChanges();
            return user;
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

        // Forgot Password Methods
        public bool UpdateResetPasswordToken(string email, string token, DateTime expiry)
        {
            try
            {
                var user = dbcontext.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return false;
                }

                user.ResetPasswordToken = token;
                user.ResetPasswordTokenExpiry = expiry;
                
                dbcontext.Users.Update(user);
                dbcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating reset password token: " + ex.Message);
            }
        }

        public User? GetUserByResetToken(string token)
        {
            return dbcontext.Users.FirstOrDefault(u => u.ResetPasswordToken == token && 
                                                      u.ResetPasswordTokenExpiry > DateTime.Now);
        }

        public bool ResetPassword(string token, string newPassword)
        {
            try
            {
                var user = GetUserByResetToken(token);
                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;

                dbcontext.Users.Update(user);
                dbcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error resetting password: " + ex.Message);
            }
        }
    }
}

