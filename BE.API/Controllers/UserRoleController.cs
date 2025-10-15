using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleRepo _userRoleRepo;

        public UserRoleController(IUserRoleRepo userRoleRepo)
        {
            _userRoleRepo = userRoleRepo;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserRoleResponse>> GetAllUserRoles()
        {
            try
            {
                var userRoles = _userRoleRepo.GetAllUserRoles();
                var response = userRoles.Select(role => new UserRoleResponse
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    CreatedDate = role.CreatedDate,
                    UserCount = role.Users?.Count ?? 0
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<UserRoleResponse> GetUserRoleById(int id)
        {
            try
            {
                var userRole = _userRoleRepo.GetUserRoleById(id);
                if (userRole == null)
                {
                    return NotFound("User role not found");
                }

                var response = new UserRoleResponse
                {
                    RoleId = userRole.RoleId,
                    RoleName = userRole.RoleName,
                    CreatedDate = userRole.CreatedDate,
                    UserCount = userRole.Users?.Count ?? 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<UserRoleResponse> CreateUserRole([FromBody] UserRoleRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RoleName))
                {
                    return BadRequest("Role name is required");
                }

                // Check if role name already exists
                var existingRole = _userRoleRepo.GetUserRoleByName(request.RoleName);
                if (existingRole != null)
                {
                    return Conflict("Role name already exists");
                }

                var userRole = new UserRole
                {
                    RoleName = request.RoleName
                };

                var createdRole = _userRoleRepo.CreateUserRole(userRole);

                var response = new UserRoleResponse
                {
                    RoleId = createdRole.RoleId,
                    RoleName = createdRole.RoleName,
                    CreatedDate = createdRole.CreatedDate,
                    UserCount = 0
                };

                return CreatedAtAction(nameof(GetUserRoleById), new { id = createdRole.RoleId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<UserRoleResponse> UpdateUserRole(int id, [FromBody] UserRoleRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RoleName))
                {
                    return BadRequest("Role name is required");
                }

                var existingRole = _userRoleRepo.GetUserRoleById(id);
                if (existingRole == null)
                {
                    return NotFound("User role not found");
                }

                // Check if new role name already exists (excluding current role)
                var duplicateRole = _userRoleRepo.GetUserRoleByName(request.RoleName);
                if (duplicateRole != null && duplicateRole.RoleId != id)
                {
                    return Conflict("Role name already exists");
                }

                existingRole.RoleName = request.RoleName;
                var updatedRole = _userRoleRepo.UpdateUserRole(existingRole);

                var response = new UserRoleResponse
                {
                    RoleId = updatedRole.RoleId,
                    RoleName = updatedRole.RoleName,
                    CreatedDate = updatedRole.CreatedDate,
                    UserCount = updatedRole.Users?.Count ?? 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUserRole(int id)
        {
            try
            {
                var existingRole = _userRoleRepo.GetUserRoleById(id);
                if (existingRole == null)
                {
                    return NotFound("User role not found");
                }

                // Check if role is being used by any users
                if (existingRole.Users?.Count > 0)
                {
                    return BadRequest("Cannot delete role that is assigned to users");
                }

                var result = _userRoleRepo.DeleteUserRole(id);
                if (!result)
                {
                    return NotFound("User role not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
