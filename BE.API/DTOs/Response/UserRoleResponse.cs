namespace BE.API.DTOs.Response
{
    public class UserRoleResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public int UserCount { get; set; }
    }
}
