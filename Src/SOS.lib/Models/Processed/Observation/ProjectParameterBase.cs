﻿namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Artportalen project parameter.
    /// </summary>
    public class ProjectParameterBase
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Project description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unit for this species observation project parameter..
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Data type.
        /// </summary>
        public string DataType { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}