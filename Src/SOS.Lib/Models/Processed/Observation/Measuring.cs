using SOS.Lib.Enums;

namespace SOS.Lib.Models.Processed.Observation
{
    public class Measuring
    {
        /// <summary>
        /// Value for measured weather variable.
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Unit for a reported measurement (given in the attribute "vädermått").
        /// </summary>
        public Unit? Unit { get; set; }
    }
}
