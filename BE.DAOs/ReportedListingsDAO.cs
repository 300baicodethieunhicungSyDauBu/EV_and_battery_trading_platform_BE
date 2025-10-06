using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.DAOs
{
    public class ReportedListingsDAO
    {
        private static ReportedListingsDAO? instance;
        private static readonly object lockObject = new object();

        public static ReportedListingsDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new ReportedListingsDAO();
                        }
                    }
                }
                return instance;
            }
        }

        private ReportedListingsDAO() { }

        public List<ReportedListing> GetAllReportedListings()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.ReportedListings
                .Include(r => r.Product)
                .Include(r => r.Reporter)
                .ToList();
        }

        public ReportedListing? GetReportedListingById(int reportId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.ReportedListings
                .Include(r => r.Product)
                .Include(r => r.Reporter)
                .FirstOrDefault(r => r.ReportId == reportId);
        }

        public List<ReportedListing> GetReportedListingsByProductId(int productId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.ReportedListings
                .Include(r => r.Product)
                .Include(r => r.Reporter)
                .Where(r => r.ProductId == productId)
                .ToList();
        }

        public List<ReportedListing> GetReportedListingsByReporterId(int reporterId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.ReportedListings
                .Include(r => r.Product)
                .Include(r => r.Reporter)
                .Where(r => r.ReporterId == reporterId)
                .ToList();
        }

        public List<ReportedListing> GetReportedListingsByStatus(string status)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.ReportedListings
                .Include(r => r.Product)
                .Include(r => r.Reporter)
                .Where(r => r.Status == status)
                .ToList();
        }

        public ReportedListing CreateReportedListing(ReportedListing reportedListing)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            reportedListing.CreatedDate = DateTime.Now;
            reportedListing.Status = "Pending";
            context.ReportedListings.Add(reportedListing);
            context.SaveChanges();
            return reportedListing;
        }

        public ReportedListing UpdateReportedListing(ReportedListing reportedListing)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var existingReport = context.ReportedListings.FirstOrDefault(r => r.ReportId == reportedListing.ReportId);
            if (existingReport != null)
            {
                existingReport.Status = reportedListing.Status;
                context.SaveChanges();
                return existingReport;
            }
            return reportedListing;
        }

        public bool DeleteReportedListing(int reportId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var report = context.ReportedListings.FirstOrDefault(r => r.ReportId == reportId);
            if (report != null)
            {
                context.ReportedListings.Remove(report);
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

