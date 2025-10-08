using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IUserRepo
    {
        User GetAccountByEmailAndPassword(string email, string password);
        User Register(User user);
        List<User> GetAllUsers();
        User GetUserById(int userId);
        User UpdateUser(User user);
        bool DeleteUser(int userId);
        
        // Forgot Password Methods
        User? GetUserByEmail(string email);
        bool UpdateResetPasswordToken(string email, string token, DateTime expiry);
        User? GetUserByResetToken(string token);
        bool ResetPassword(string token, string newPassword);
    }
}
