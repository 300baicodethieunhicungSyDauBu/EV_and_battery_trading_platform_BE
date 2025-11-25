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
    public class UserRepo : IUserRepo
    {
        public User GetAccountByEmailAndPassword(string email, string password)
        {
            return UserDAO.Instance.GetAccountByEmailAndPassword(email, password);
        }

        public User Register(User user)
        {
            return UserDAO.Instance.Register(user);
        }

        public List<User> GetAllUsers()
        {
            return UserDAO.Instance.GetAllUsers();
        }

        public User GetUserById(int userId)
        {
            return UserDAO.Instance.GetUserById(userId);
        }

        public User UpdateUser(User user)
        {
            return UserDAO.Instance.UpdateUser(user);
        }

        public bool DeleteUser(int userId)
        {
            return UserDAO.Instance.DeleteUser(userId);
        }

        // Forgot Password Methods
        public User? GetUserByEmail(string email)
        {
            return UserDAO.Instance.GetUserByEmail(email);
        }

        public bool UpdateResetPasswordToken(string email, string token, DateTime expiry)
        {
            return UserDAO.Instance.UpdateResetPasswordToken(email, token, expiry);
        }

        public User? GetUserByResetToken(string token)
        {
            return UserDAO.Instance.GetUserByResetToken(token);
        }

        public bool ResetPassword(string token, string newPassword)
        {
            return UserDAO.Instance.ResetPassword(token, newPassword);
        }
    }
}
