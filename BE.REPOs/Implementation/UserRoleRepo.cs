using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation
{
    public class UserRoleRepo : IUserRoleRepo
    {
        public List<UserRole> GetAllUserRoles()
        {
            return UserRoleDAO.Instance.GetAllUserRoles();
        }

        public UserRole? GetUserRoleById(int roleId)
        {
            return UserRoleDAO.Instance.GetUserRoleById(roleId);
        }

        public UserRole? GetUserRoleByName(string roleName)
        {
            return UserRoleDAO.Instance.GetUserRoleByName(roleName);
        }

        public UserRole CreateUserRole(UserRole userRole)
        {
            return UserRoleDAO.Instance.CreateUserRole(userRole);
        }

        public UserRole UpdateUserRole(UserRole userRole)
        {
            return UserRoleDAO.Instance.UpdateUserRole(userRole);
        }

        public bool DeleteUserRole(int roleId)
        {
            return UserRoleDAO.Instance.DeleteUserRole(roleId);
        }
    }
}
