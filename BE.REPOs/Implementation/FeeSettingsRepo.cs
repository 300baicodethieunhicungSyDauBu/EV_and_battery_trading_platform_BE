using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation
{
    public class FeeSettingsRepo : IFeeSettings
    {
        public List<FeeSetting> GetAllFeeSettings()
        {
            return FeeSettingsDAO.Instance.GetAllFeeSettings();
        }

        public FeeSetting? GetFeeSettingById(int feeId)
        {
            return FeeSettingsDAO.Instance.GetFeeSettingById(feeId);
        }

        public List<FeeSetting> GetActiveFeeSettings()
        {
            return FeeSettingsDAO.Instance.GetActiveFeeSettings();
        }

        public List<FeeSetting> GetFeeSettingsByType(string feeType)
        {
            return FeeSettingsDAO.Instance.GetFeeSettingsByType(feeType);
        }

        public FeeSetting CreateFeeSetting(FeeSetting feeSetting)
        {
            return FeeSettingsDAO.Instance.CreateFeeSetting(feeSetting);
        }

        public FeeSetting UpdateFeeSetting(FeeSetting feeSetting)
        {
            return FeeSettingsDAO.Instance.UpdateFeeSetting(feeSetting);
        }

        public bool DeleteFeeSetting(int feeId)
        {
            return FeeSettingsDAO.Instance.DeleteFeeSetting(feeId);
        }
    }
}
