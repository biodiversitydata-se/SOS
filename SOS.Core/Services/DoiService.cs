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

        public async Task<DoiInfo> CreateDoiAsync(
            IEnumerable<ObservationVersionIdentifier> observationIdentifiers,
            DoiMetadata doiMetadata)
        {
            byte[] serializedObservationIdentifiers = MessagePackSerializer.Serialize(observationIdentifiers);
            DoiInfo doiInfo = DoiInfo.Create(doiMetadata);
            await _doiRepository.InsertDoiFileAsync(serializedObservationIdentifiers, doiInfo.DoiId, doiInfo.FileName);
            await _doiRepository.InsertDoiDocumentAsync(doiInfo);
            return doiInfo;
        }

        public async Task<IList<ProcessedDwcObservation>> GetDoiObservationsAsync(string filename)
        {
            var observationVersionIdentifiers = await GetDoiObservationIdentifiersAsync(filename);
            var observations = await _observationRepository.RestoreDocumentsAsync(observationVersionIdentifiers);
            return observations;
        }

        public async Task<IEnumerable<ObservationVersionIdentifier>> GetDoiObservationIdentifiersAsync(string filename)
        {
            var messagePackFile = await _doiRepository.GetDoiFileByNameAsync(filename);
            var res = MessagePackSerializer.Deserialize<IEnumerable<ObservationVersionIdentifier>>(messagePackFile);
            return res;
        }
    }
}