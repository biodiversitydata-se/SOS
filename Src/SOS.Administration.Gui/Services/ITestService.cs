using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Services
{
    public interface ITestService
    {
        IEnumerable<Test> GetTests();
        Task<TestResults> Test_DataProviders();
        Task<TestResults> Test_GeoGridAggregation();
        Task<TestResults> Test_SearchOtter();
        Task<TestResults> Test_SearchOtterAtLocation();
        Task<TestResults> Test_SearchWolf();
        Task<TestResults> Test_TaxonAggregation();
        Task<TestResults> Test_TaxonAggregationBBox();
        Task<TestResults> Test_Vocabulary();
    }
}