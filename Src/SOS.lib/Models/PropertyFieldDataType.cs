namespace SOS.Lib.Models
{
    public enum PropertyFieldDataType
    {
        /// <summary>
        /// A Boolean value.
        /// String representation of a Boolean value is
        /// either True or False.
        /// </summary>
        Boolean = 0,

        /// <summary>
        /// A DateTime value.
        /// String representation of a DateTime value is in the format
        /// Year-Month-DayTHour:Minute:Second:PartOfSecond, example
        /// 2008-06-15T21:15:07.0000000.
        /// </summary>
        DateTime = 1,

        /// <summary>
        /// A Float value. Standard 64-bit floating point.
        /// String representation of a Float64 value is in the format
        /// 3.1415926536E+000.
        /// </summary>
        Double = 2,

        /// <summary>
        /// An Int32 value.
        /// String representation of a Int32 value is in the format
        /// 2147483647.
        /// </summary>
        Int32 = 3,

        /// <summary>
        /// An Int64 value.
        /// String representation of a Int64 value is in the format
        /// 9223372036854775807.
        /// </summary>
        Int64 = 4,

        /// <summary>
        /// A String value.
        /// </summary>
        String = 5
    }
}