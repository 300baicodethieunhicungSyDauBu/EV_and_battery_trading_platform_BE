using BE.BOs.Models;

namespace BE.REPOs.Interface
{
    public interface IReportedListingsRepo
    {
        List<ReportedListing> GetAllReportedListings();
        ReportedListing? GetReportedListingById(int reportId);
        List<ReportedListing> GetReportedListingsByProductId(int productId);
        List<ReportedListing> GetReportedListingsByReporterId(int reporterId);
        List<ReportedListing> GetReportedListingsByStatus(string status);
        ReportedListing CreateReportedListing(ReportedListing reportedListing);
        ReportedListing UpdateReportedListing(ReportedListing reportedListing);
        bool DeleteReportedListing(int reportId);
    }
}
