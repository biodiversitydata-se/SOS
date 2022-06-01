using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class MediaRepository : BaseRepository<MediaRepository>, IMediaRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenDataService"></param>
        /// <param name="logger"></param>
        public MediaRepository(IArtportalenDataService artportalenDataService, ILogger<MediaRepository> logger) : base(
            artportalenDataService, logger)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MediaEntity>> GetAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                if (!sightingIds?.Any() ?? true)
                {
                    return null!;
                }

                var query = @" 
                SELECT          
                    mf.Id,
	                mf.SightingID,
                    FileUri,
	                UploadDateTime,
	                CopyrightText,
	                t.Value AS [FileType],
	                p.FirstName + ' ' + p.LastName AS RightsHolder,
					mfc.MessageText AS Comment,
					mfc.CreationTime AS CommentCreated,
					mp.FirstName + ' ' + mp.LastName AS CommentBy
                 FROM 
					MediaFile mf
                    INNER JOIN @tvp tvp ON mf.SightingID = tvp.Id
					LEFT JOIN MediaFileType mft ON mf.MediaFileTypeId = mft.Id
	                LEFT JOIN Resource R ON mft.ResourceLabel = r.Label 
	                LEFT JOIN Translation t ON r.Id = t.ResourceId AND t.GlobalizationCultureId = 49
	                LEFT JOIN [User] u ON mf.UserId = u.Id
	                LEFT JOIN Person p ON u.PersonId = p.Id
					LEFT JOIN MediaFileComment mfc ON mfc.MediaFileId = mf.Id AND mfc.Deleted = 0
					LEFT JOIN [User] mu ON mfc.UserId = mu.Id
	                LEFT JOIN Person mp ON mu.PersonId = mp.Id";

                return await QueryAsync<MediaEntity>(query,
                    new { tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable") });

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting media files");
                return null!;
            }
        }
    }
}