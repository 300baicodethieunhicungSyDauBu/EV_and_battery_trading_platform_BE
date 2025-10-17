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
    public class FeeSettingController : ControllerBase
    {
        private readonly IFeeSettingsRepo _feeSettingsRepoRepo;

        public FeeSettingController(IFeeSettingsRepo feeSettingsRepoRepo)
        {
            _feeSettingsRepoRepo = feeSettingsRepoRepo;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<FeeSettingResponse>> GetAllFeeSettings()
        {
            try
            {
                var feeSettings = _feeSettingsRepoRepo.GetAllFeeSettings();
                var response = feeSettings.Select(fee => new FeeSettingResponse
                {
                    FeeId = fee.FeeId,
                    FeeType = fee.FeeType,
                    FeeValue = fee.FeeValue,
                    IsActive = fee.IsActive ?? false,
                    CreatedDate = fee.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<FeeSettingResponse> GetFeeSettingById(int id)
        {
            try
            {
                var feeSetting = _feeSettingsRepoRepo.GetFeeSettingById(id);
                if (feeSetting == null)
                {
                    return NotFound("Fee setting not found");
                }

                var response = new FeeSettingResponse
                {
                    FeeId = feeSetting.FeeId,
                    FeeType = feeSetting.FeeType,
                    FeeValue = feeSetting.FeeValue,
                    IsActive = feeSetting.IsActive ?? false,
                    CreatedDate = feeSetting.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("active")]
        public ActionResult<IEnumerable<FeeSettingResponse>> GetActiveFeeSettings()
        {
            try
            {
                var feeSettings = _feeSettingsRepoRepo.GetActiveFeeSettings();
                var response = feeSettings.Select(fee => new FeeSettingResponse
                {
                    FeeId = fee.FeeId,
                    FeeType = fee.FeeType,
                    FeeValue = fee.FeeValue,
                    IsActive = fee.IsActive ?? false,
                    CreatedDate = fee.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("type/{feeType}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<FeeSettingResponse>> GetFeeSettingsByType(string feeType)
        {
            try
            {
                var feeSettings = _feeSettingsRepoRepo.GetFeeSettingsByType(feeType);
                var response = feeSettings.Select(fee => new FeeSettingResponse
                {
                    FeeId = fee.FeeId,
                    FeeType = fee.FeeType,
                    FeeValue = fee.FeeValue,
                    IsActive = fee.IsActive ?? false,
                    CreatedDate = fee.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<FeeSettingResponse> CreateFeeSetting([FromBody] FeeSettingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FeeType))
                {
                    return BadRequest("Fee type is required");
                }

                if (request.FeeValue < 0)
                {
                    return BadRequest("Fee value must be non-negative");
                }

                var feeSetting = new FeeSetting
                {
                    FeeType = request.FeeType,
                    FeeValue = request.FeeValue,
                    IsActive = request.IsActive
                };

                var createdFeeSetting = _feeSettingsRepoRepo.CreateFeeSetting(feeSetting);

                var response = new FeeSettingResponse
                {
                    FeeId = createdFeeSetting.FeeId,
                    FeeType = createdFeeSetting.FeeType,
                    FeeValue = createdFeeSetting.FeeValue,
                    IsActive = createdFeeSetting.IsActive ?? false,
                    CreatedDate = createdFeeSetting.CreatedDate
                };

                return CreatedAtAction(nameof(GetFeeSettingById), new { id = createdFeeSetting.FeeId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<FeeSettingResponse> UpdateFeeSetting(int id, [FromBody] FeeSettingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FeeType))
                {
                    return BadRequest("Fee type is required");
                }

                if (request.FeeValue < 0)
                {
                    return BadRequest("Fee value must be non-negative");
                }

                var existingFeeSetting = _feeSettingsRepoRepo.GetFeeSettingById(id);
                if (existingFeeSetting == null)
                {
                    return NotFound("Fee setting not found");
                }

                existingFeeSetting.FeeType = request.FeeType;
                existingFeeSetting.FeeValue = request.FeeValue;
                existingFeeSetting.IsActive = request.IsActive;

                var updatedFeeSetting = _feeSettingsRepoRepo.UpdateFeeSetting(existingFeeSetting);

                var response = new FeeSettingResponse
                {
                    FeeId = updatedFeeSetting.FeeId,
                    FeeType = updatedFeeSetting.FeeType,
                    FeeValue = updatedFeeSetting.FeeValue,
                    IsActive = updatedFeeSetting.IsActive ?? false,
                    CreatedDate = updatedFeeSetting.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult DeleteFeeSetting(int id)
        {
            try
            {
                var result = _feeSettingsRepoRepo.DeleteFeeSetting(id);
                if (!result)
                {
                    return NotFound("Fee setting not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        
        [HttpGet("active/{feeType}")]
        [AllowAnonymous]
        public ActionResult<decimal> GetActiveFeeValue(string feeType)
        {
            try
            {
                var fee = _feeSettingsRepoRepo.GetActiveFeeSettings()
                    .FirstOrDefault(f => f.FeeType.Equals(feeType, StringComparison.OrdinalIgnoreCase));

                if (fee == null)
                    return NotFound($"No active fee found for type: {feeType}");

                return Ok(fee.FeeValue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
