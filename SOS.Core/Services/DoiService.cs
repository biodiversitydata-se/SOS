using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using SOS.Core.Models.DOI;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;

namespace SOS.Core.Services
{
    public class DoiService : IDoiService
    {
        private readonly IVersionedObservationRepository<ProcessedDwcObservation> _observationRepository;
        private readonly IDoiRepository _doiRepository;

        public DoiService(
            IVersionedObservationRepository<ProcessedDwcObservation> observationRepository,
            IDoiRepository doiRepository)
        {
            _observationRepository = observationRepository;
            _doiRepository = doiRepository;
        }

        public async Task<DoiInfo> CreateDoiWithAllObservationsAsync(DoiMetadata doiMetadata)
        {
            byte[] file = CreateFileWithAllObservations();
            DoiInfo doiInfo = DoiInfo.Create(doiMetadata);
            await _doiRepository.InsertDoiFileAsync(file, doiInfo.DoiId, doiInfo.FileName);
            await _doiRepository.InsertDoiDocumentAsync(doiInfo);
            return doiInfo;
        }

        public async Task<IEnumerable<ObservationVersionIdentifier>> GetDoiObservationsAsync(string filename)
        {
            var messagePackFile = await _doiRepository.GetDoiFileByNameAsync(filename);
            var res = MessagePackSerializer.Deserialize<IEnumerable<ObservationVersionIdentifier>>(messagePackFile);
            return res;
        }

        private byte[] CreateFileWithAllObservations()
        {
            var observations = _observationRepository.GetAllObservationVersionIdentifiersEnumerable();
            var bin = MessagePackSerializer.Serialize(observations);
            return bin;
        }
    }
}