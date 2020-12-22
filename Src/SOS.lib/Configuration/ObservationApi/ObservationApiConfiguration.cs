namespace SOS.Lib.Configuration.ObservationApi
{
    public class ObservationApiConfiguration
    {
        public int  ExportObservationsLimit { get; set; }

        /// <summary>
        /// Max calculated tiles returned
        /// </summary>
        public int TilesLimit { get; set; }
    }
}