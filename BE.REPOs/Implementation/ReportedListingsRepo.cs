using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation
{
    public class ReportedListingsRepo : IReportedListingsRepo
    {
        public List<ReportedListing> GetAllReportedListings()
        {
            return ReportedListingsDAO.Instance.GetAllReportedListings();
        }

        public ReportedListing? GetReportedListingById(int reportId)
        {
            return ReportedListingsDAO.Instance.GetReportedListingById(reportId);
        }

        public List<ReportedListing> GetReportedListingsByProductId(int productId)
        {
            return ReportedListingsDAO.Instance.GetReportedListingsByProductId(productId);
        }

        public List<ReportedListing> GetReportedListingsByReporterId(int reporterId)
        {
            return ReportedListingsDAO.Instance.GetReportedListingsByReporterId(reporterId);
        }

        public List<ReportedListing> GetReportedListingsByStatus(string status)
        {
            return ReportedListingsDAO.Instance.GetReportedListingsByStatus(status);
        }

        public ReportedListing CreateReportedListing(ReportedListing reportedListing)
        {
            return ReportedListingsDAO.Instance.CreateReportedListing(reportedListing);
        }

        public ReportedListing UpdateReportedListing(ReportedListing reportedListing)
        {
            return ReportedListingsDAO.Instance.UpdateReportedListing(reportedListing);
        }

        public bool DeleteReportedListing(int reportId)
        {
            return ReportedListingsDAO.Instance.DeleteReportedListing(reportId);
        }
    }
}
