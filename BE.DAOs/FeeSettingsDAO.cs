using BE.BOs.Models;

namespace BE.DAOs
{
    public class FeeSettingsDAO
    {
        private static FeeSettingsDAO? instance;
        private static readonly object lockObject = new object();

        public static FeeSettingsDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new FeeSettingsDAO();
                        }
                    }
                }
                return instance;
            }
        }

        private FeeSettingsDAO() { }

        public List<FeeSetting> GetAllFeeSettings()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.FeeSettings.ToList();
        }

        public FeeSetting? GetFeeSettingById(int feeId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.FeeSettings.FirstOrDefault(f => f.FeeId == feeId);
        }

        public List<FeeSetting> GetActiveFeeSettings()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.FeeSettings.Where(f => f.IsActive == true).ToList();
        }

        public List<FeeSetting> GetFeeSettingsByType(string feeType)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.FeeSettings.Where(f => f.FeeType == feeType).ToList();
        }

        public FeeSetting CreateFeeSetting(FeeSetting feeSetting)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            feeSetting.CreatedDate = DateTime.Now;
            context.FeeSettings.Add(feeSetting);
            context.SaveChanges();
            return feeSetting;
        }

        public FeeSetting UpdateFeeSetting(FeeSetting feeSetting)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var existingFeeSetting = context.FeeSettings.FirstOrDefault(f => f.FeeId == feeSetting.FeeId);
            if (existingFeeSetting != null)
            {
                existingFeeSetting.FeeType = feeSetting.FeeType;
                existingFeeSetting.FeeValue = feeSetting.FeeValue;
                existingFeeSetting.IsActive = feeSetting.IsActive;
                context.SaveChanges();
                return existingFeeSetting;
            }
            return feeSetting;
        }

        public bool DeleteFeeSetting(int feeId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var feeSetting = context.FeeSettings.FirstOrDefault(f => f.FeeId == feeId);
            if (feeSetting != null)
            {
                context.FeeSettings.Remove(feeSetting);
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

