using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Core.Models.DOI;

namespace SOS.Core.Repositories
{
    public interface IDoiRepository
    {
        Task InsertDoiDocumentAsync(DoiInfo doiInfo);
        Task<DoiInfo> GetDocumentAsync(string doiId);
        Task<ObjectId> InsertDoiFileAsync(byte[] doiFile, string doiId, string doiFilename);
        Task<byte[]> GetDoiFileByNameAsync(string fileName);
        Task<byte[]> GetDoiFileAsync(ObjectId id);
    }
}