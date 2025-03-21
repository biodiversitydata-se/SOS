using NetTopologySuite.Geometries;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Shared.Api.Validators.Interfaces
{
    public interface IInputValidator
    {
        /// <summary>
        /// Validate paging args
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="handleInfinityTake"></param>
        /// <returns></returns>
        Result ValidateAggregationPagingArguments(int skip, int? take, bool handleInfinityTake = false);

        /// <summary>
        /// Validate bounding box
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        Result ValidateBoundingBox(
            LatLonBoundingBoxDto? boundingBox,
            bool mandatory = false);

        /// <summary>
        /// Validate double
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        Result ValidateDouble(double value, double minLimit, double maxLimit, string paramName);

        /// <summary>
        /// Validate email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Result ValidateEmail(string email);

        /// <summary>
        /// Validate encrypt password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="confirmPassword"></param>
        /// <param name="protectionFilter"></param>
        /// <returns></returns>
        Result ValidateEncryptPassword(string password, string confirmPassword, ProtectionFilterDto protectionFilter);

        /// <summary>
        /// Validate fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        Result ValidateFields(IEnumerable<string> fields);

        /// <summary>
        /// Make sure geographical data 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Result ValidateGeographicalAreaExists(GeographicsFilterDto filter);

        /// <summary>
        /// Validate zoom args
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        /// <returns></returns>
        Result ValidateGeogridZoomArgument(int zoom, int minLimit, int maxLimit);

        /// <summary>
        /// Validate geometries
        /// </summary>
        /// <param name="geometries"></param>
        /// <returns></returns>
        Result ValidateGeometries(
            IEnumerable<Geometry> geometries);

        /// <summary>
        /// Validate grid cell size in m
        /// </summary>
        /// <param name="gridCellSizeInMeters"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        /// <returns></returns>
        Result ValidateGridCellSizeInMetersArgument(int gridCellSizeInMeters, int minLimit, int maxLimit);

        /// <summary>
        /// Validate integer
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        Result ValidateInt(int value, int minLimit, int maxLimit, string paramName);

        /// <summary>
        /// Validate property exists
        /// </summary>
        /// <param name="name"></param>
        /// <param name="property"></param>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        Result ValidatePropertyExists(string name, string property, bool mandatory = false);

        /// <summary>
        /// Validate serach filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="bboxMandatory"></param>
        /// <returns></returns>
        Task<Result> ValidateSearchFilterAsync(SearchFilterBaseDto filter, bool allowObjectInOutputFields = true, bool bboxMandatory = false);

        /// <summary>
        /// Validate Search paging arguments
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Result ValidateSearchPagingArguments(int skip, int take);

        /// <summary>
        /// Validate search paging argumets internal
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Result ValidateSearchPagingArgumentsInternal(int skip, int take);

        /// <summary>
        ///  Validate signal search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="areaBuffer"></param>
        /// <returns></returns>
        Task<Result> ValidateSignalSearchAsync(SignalFilterDto filter, bool validateSearchFilter, int areaBuffer);


        /// <summary>
        /// Validate sort fields
        /// </summary>
        /// <param name="sortFields"></param>
        /// <returns></returns>
        Task<Result> ValidateSortFieldsAsync(IEnumerable<string> sortFields);

        /// <summary>
        /// Validate taxa
        /// </summary>
        /// <param name="taxonIds"></param>
        /// <returns></returns>
        Task<Result> ValidateTaxaAsync(IEnumerable<int> taxonIds);

        /// <summary>
        /// Make sure filter contains taxa
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Result ValidateTaxonExists(SearchFilterBaseDto filter);

        /// <summary>
        /// Validate taxon aggregation paging arguments
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Result ValidateTaxonAggregationPagingArguments(int? skip, int? take);

        /// <summary>
        /// Validate tiles limit
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="zoom"></param>
        /// <param name="countTask"></param>
        /// <param name="internalLimit"></param>
        /// <returns></returns>
        Task<Result> ValidateTilesLimitAsync(
            Envelope envelope,
            int zoom,
            Task<long> countTask,
            bool internalLimit = false);

        /// <summary>
        /// Validate metric tiles limit
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <param name="countTask"></param>
        /// <param name="internalLimit"></param>
        /// <param name="internalLimitFactor"></param>
        /// <returns></returns>
        Task<Result> ValidateTilesLimitMetricAsync(
            Envelope envelope,
            int gridCellSizeInMeters,
            Task<long> countTask,
            bool internalLimit = false,
            double internalLimitFactor = 1.0);

        /// <summary>
        /// Validate translation culture code
        /// </summary>
        /// <param name="translationCultureCode"></param>
        /// <returns></returns>
        Result ValidateTranslationCultureCode(string translationCultureCode);
    }
}
