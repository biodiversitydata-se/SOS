//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SOS.Core.Tests.SampleCode
//{
//    public class IterateMongoDbDocuments
//    {
//        public async Task<List<ObservationVersionIdentifier>> GetAllObservationVersionIdentifiersUsingBatches()
//        {
//            List<ObservationVersionIdentifier> observationVersionIdentifiers = new List<ObservationVersionIdentifier>();
//            var filterDef = new FilterDefinitionBuilder<VersionedObservation<T>>();
//            var filter = filterDef.Empty;
//            var projection = Builders<VersionedObservation<T>>.Projection
//                .Include(x => x.Id)
//                .Include(x => x.DataProviderId)
//                .Include(x => x.CatalogNumber)
//                .Include(x => x.Version);
//            var options = new FindOptions<VersionedObservation<T>, ObservationVersionIdentifier>
//            {
//                Projection = projection,
//                BatchSize = 100
//            };
//            using (var cursor = await Collection.FindAsync(filter, options))
//            {
//                // Alternativ 1
//                //// Move to the next batch of docs
//                //while (await cursor.MoveNextAsync())
//                //{
//                //    var batch = cursor.Current;
//                //    foreach (var doc in batch)
//                //    {
//                //        // process doc
//                //        observationVersionIdentifiers.Add(doc);
//                //    }
//                //}

//                // Alternativ 2
//                await cursor.ForEachAsync(doc =>
//                {
//                    // process doc
//                    observationVersionIdentifiers.Add(doc);
//                });
//            }

//            return observationVersionIdentifiers;
//        }


//        public IEnumerable<ObservationVersionIdentifier> GetAllObservationVersionIdentifiersEnumerable2()
//        {
//            List<ObservationVersionIdentifier> observationVersionIdentifiers = new List<ObservationVersionIdentifier>();
//            var filterDef = new FilterDefinitionBuilder<VersionedObservation<T>>();
//            var filter = filterDef.Empty;
//            var projection = Builders<VersionedObservation<T>>.Projection
//                .Include(x => x.Id)
//                .Include(x => x.DataProviderId)
//                .Include(x => x.CatalogNumber)
//                .Include(x => x.Version);
//            var options = new FindOptions<VersionedObservation<T>, ObservationVersionIdentifier>
//            {
//                Projection = projection,
//                BatchSize = 100
//            };

//            using (var cursor = Collection.FindAsync(filter, options))
//            {
//                // Alternativ 1
//                //// Move to the next batch of docs
//                while (cursor.Result.MoveNext())
//                {
//                    var batch = cursor.Result.Current;
//                    foreach (var doc in batch)
//                    {
//                        yield return doc;
//                    }
//                }
//            }
//        }

//public async Task<List<ObservationVersionIdentifier>> GetAllObservationVersionIdentifiersUsingFilterDefBuilder()
//{
//var filterDef = new FilterDefinitionBuilder<VersionedObservation<T>>();
//var filter = filterDef.Empty;
//var projection = Builders<VersionedObservation<T>>.Projection
//    .Include(x => x.Id)
//    .Include(x => x.DataProviderId)
//    .Include(x => x.CatalogNumber)
//    .Include(x => x.Version);
//var options = new FindOptions<VersionedObservation<T>, ObservationVersionIdentifier> { Projection = projection };
//var observationVersionIdentifiers = await (await Collection.FindAsync(filter, options)).ToListAsync();
//    return observationVersionIdentifiers;
//}


//        public async Task<List<ObservationVersionIdentifier>> GetAllObservationVersionIdentifiersUsingPaging()
//        {
//            int currentPage = 1;
//            int pageSize = 2;
//            List<ObservationVersionIdentifier> observationVersionIdentifiers = new List<ObservationVersionIdentifier>();

//            double totalDocuments = await Collection.CountAsync(FilterDefinition<VersionedObservation<T>>.Empty);
//            var totalPages = Math.Ceiling(totalDocuments / pageSize);

//            for (int i = 1; i <= totalPages; i++)
//            {
//                int count = 1;
//                await Collection.Find(FilterDefinition<VersionedObservation<T>>.Empty)
//                    .Skip((currentPage - 1) * pageSize)
//                    .Limit(pageSize)
//                    .Project(s => new ObservationVersionIdentifier
//                    {
//                        Id = s.Id.ToString(),
//                        CatalogNumber = s.CatalogNumber,
//                        DataProviderId = s.DataProviderId,
//                        Version = s.Version
//                    })
//                    .ForEachAsync(
//                        x =>
//                        {
//                            observationVersionIdentifiers.Add(x);
//                            count++;
//                        });

//                currentPage++;
//            }

//            return observationVersionIdentifiers;
//        }

//        public async Task<List<ObservationVersionIdentifier>> GetAllObservationVersionIdentifiersUsingPaging2()
//        {
//            int currentPage = 1;
//            int pageSize = 2;
//            List<ObservationVersionIdentifier> observationVersionIdentifiers = new List<ObservationVersionIdentifier>();

//            double totalDocuments = await Collection.CountAsync(FilterDefinition<VersionedObservation<T>>.Empty);
//            var totalPages = Math.Ceiling(totalDocuments / pageSize);

//            for (int i = 1; i <= totalPages; i++)
//            {
//                int count = 1;
//                await Collection.Find(FilterDefinition<VersionedObservation<T>>.Empty)
//                    .Skip((currentPage - 1) * pageSize)
//                    .Limit(pageSize)
//                    .Project(s => new ObservationVersionIdentifier
//                    {
//                        Id = s.Id.ToString(),
//                        CatalogNumber = s.CatalogNumber,
//                        DataProviderId = s.DataProviderId,
//                        Version = s.Version
//                    })
//                    .ForEachAsync(
//                        x =>
//                        {
//                            observationVersionIdentifiers.Add(x);
//                            count++;
//                        });

//                currentPage++;
//            }

//            return observationVersionIdentifiers;
//        }

//        //private IAsyncCursor<MongoAlbum> GetCursor()
//        //{
//        //    var filter = MakeFilter();
//        //    var cursor = Collection.FindAsync(filter).Result;
//        //    return cursor;
//        //}

//        //internal IEnumerable<string> IterateThroughAlbumIds()
//        //{
//        //    using (var Cursor = GetCursor())
//        //    {
//        //        while (Cursor.MoveNextAsync().Result)
//        //        {
//        //            var Batch = Cursor.Current;
//        //            foreach (var Document in Batch) yield return Document._id;
//        //        }
//        //    }
//        //}

//        //internal IEnumerable<MongoAlbum> IterateThroughAlbums()
//        //{
//        //    using (var Cursor = GetCursor())
//        //    {
//        //        while (Cursor.MoveNextAsync().Result)
//        //        {
//        //            var Batch = Cursor.Current;
//        //            foreach (var Document in Batch) yield return Document;
//        //        }
//        //    }
//        //}

//    }
//public class ObservationVersionIdentifierEnumerator : IEnumerator<List<ObservationVersionIdentifier>>
//{
//    private int _currentPageNumber;
//    private int _pageSize = 100;

//    public bool MoveNext()
//    {
//        throw new NotImplementedException();
//    }

//    public void Reset()
//    {
//        throw new NotImplementedException();
//    }

//    public List<ObservationVersionIdentifier> Current { get; }

//    object IEnumerator.Current => Current;

//    public void Dispose()
//    {
//        throw new NotImplementedException();
//    }
//}

//}
