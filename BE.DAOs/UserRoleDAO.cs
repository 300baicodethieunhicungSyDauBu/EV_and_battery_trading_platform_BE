using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class UserRoleDAO
    {
        private static UserRoleDAO? instance;
        private static readonly object lockObject = new object();

        public static UserRoleDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new UserRoleDAO();
                        }
                    }
                }
                return instance;
            }
        }

        private UserRoleDAO() { }

        public List<UserRole> GetAllUserRoles()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.UserRoles.ToList();
        }

        public UserRole? GetUserRoleById(int roleId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.UserRoles.FirstOrDefault(r => r.RoleId == roleId);
        }

        public UserRole? GetUserRoleByName(string roleName)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.UserRoles.FirstOrDefault(r => r.RoleName == roleName);
        }

        public UserRole CreateUserRole(UserRole userRole)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            userRole.CreatedDate = DateTime.Now;
            context.UserRoles.Add(userRole);
            context.SaveChanges();
            return userRole;
        }

        public UserRole UpdateUserRole(UserRole userRole)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var existingRole = context.UserRoles.FirstOrDefault(r => r.RoleId == userRole.RoleId);
            if (existingRole != null)
            {
                existingRole.RoleName = userRole.RoleName;
                context.SaveChanges();
                return existingRole;
            }
            return userRole;
        }

        public bool DeleteUserRole(int roleId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var role = context.UserRoles.FirstOrDefault(r => r.RoleId == roleId);
            if (role != null)
            {
                context.UserRoles.Remove(role);
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
