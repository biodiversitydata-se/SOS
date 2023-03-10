namespace SOS.DataStewardship.Api.IntegrationTests.Data
{
    internal class DatasetBuilderFactory
    {       
        public static IOperable<Dataset> Create(int size = 10)
        {
            var datasetBuilder = Builder<Dataset>.CreateListOfSize(size)
                .All()
                    .With(m => m.Identifier = DataHelper.RandomString(8))
                    .With(m => m.Purpose = null)
                    .With(m => m.AccessRights = null);

            return datasetBuilder;
        }
    }
}