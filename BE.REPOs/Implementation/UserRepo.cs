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
    }
}
