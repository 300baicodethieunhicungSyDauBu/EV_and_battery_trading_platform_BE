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
        public User GetAccountByEmailAndPassword(string email, string password);
    }
}
