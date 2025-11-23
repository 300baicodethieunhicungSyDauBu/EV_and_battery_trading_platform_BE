using BE.BOs.Models;

namespace BE.REPOs.Interface
{
    public interface IFeeSettingsRepo
    {
        List<FeeSetting> GetAllFeeSettings();
        FeeSetting? GetFeeSettingById(int feeId);
        List<FeeSetting> GetActiveFeeSettings();
        List<FeeSetting> GetFeeSettingsByType(string feeType);
        FeeSetting CreateFeeSetting(FeeSetting feeSetting);
        FeeSetting UpdateFeeSetting(FeeSetting feeSetting);
        bool DeleteFeeSetting(int feeId);
        decimal GetActiveFeeValue(string feeType);
        FeeSetting? GetSingleFeeByType(string feeType);
    }
}
