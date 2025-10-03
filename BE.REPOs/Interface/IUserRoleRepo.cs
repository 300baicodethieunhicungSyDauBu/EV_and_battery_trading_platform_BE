using BE.BOs.Models;

namespace BE.REPOs.Interface
{
    public interface IUserRoleRepo
    {
        List<UserRole> GetAllUserRoles();
        UserRole? GetUserRoleById(int roleId);
        UserRole? GetUserRoleByName(string roleName);
        UserRole CreateUserRole(UserRole userRole);
        UserRole UpdateUserRole(UserRole userRole);
        bool DeleteUserRole(int roleId);
    }
}
