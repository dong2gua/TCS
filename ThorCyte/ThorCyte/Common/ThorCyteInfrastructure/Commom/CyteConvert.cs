using System;
using System.Linq;
using ThorCyte.Infrastructure.Exceptions;

namespace ThorCyte.Infrastructure.Commom
{
    public class CyteConvert
    {
        private CyteConvert()
        {
        }

        #region string parsing //--------------------------------------------------------------------

        /// <summary>
        /// converts string to Int32
        /// returns defaultValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public int ToInt32(string val, int defaultValue = 0)
        {
            try
            {
                return string.IsNullOrEmpty(val) ? defaultValue : Convert.ToInt32(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        // jcl-2456
        /// <summary>
        /// converts string to int32, rounding double values to int values
        /// returns 0 if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public int DoubleStringToInt32(string val)
        {
            return Convert.ToInt32(ToDouble(val));
        }


        /// <summary>
        /// converts string to double
        /// returns defaultValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public double ToDouble(string val, double defaultValue = 0.0)
        {
            try
            {
                return string.IsNullOrEmpty(val) ? defaultValue : Convert.ToDouble(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// converts string to float
        /// returns defaultValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public float ToSingle(string val, float defaultValue = 0)
        {
            try
            {
                return string.IsNullOrEmpty(val) ? defaultValue : Convert.ToSingle(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// converts string to byte
        /// returns defaultValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public byte ToByte(string val, byte defaultValue = 0)
        {
            try
            {
                return string.IsNullOrEmpty(val) ? defaultValue : Convert.ToByte(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// converts string to boolean
        /// returns false if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public bool ToBoolean(string val)
        {
            try
            {
                return Convert.ToBoolean(val);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// converts string to boolean
        /// returns defaultValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public bool ToBoolean(string val, bool defaultValue)
        {
            try
            {
                return string.IsNullOrEmpty(val) ? defaultValue : Convert.ToBoolean(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// returns "" if val is null, otherwise val
        /// </summary>
        static public string ToString(string val)
        {
            return ToString(val, string.Empty);
        }

        /// <summary>
        /// returns defaultValue if val is null, otherwise val
        /// </summary>
        static public string ToString(string val, string defaultValue)
        {
            return val ?? defaultValue;
        }

        static public bool IsNumber(string val)
        {
            return !val.Where((t, i) => !char.IsNumber(val, i)).Any();
        }

        /// <summary>
        /// returns defaultValue if string is null or empty, otherwise first character
        /// </summary>
        static public char ToChar(string val, char defaultValue = ' ')
        {
            return string.IsNullOrEmpty(val) ? defaultValue : val[0];
        }

        /// <summary>
        /// converts string to DateTime
        /// returns DateTime.MinValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public DateTime ToDateTime(string dateString)
        {
            return ToDateTime(dateString, DateTime.MinValue);
        }

        /// <summary>
        /// converts string to DateTime
        /// returns defaultValue if conversion not possible (eg null or invalid chars)
        /// </summary>
        static public DateTime ToDateTime(string dateString, DateTime defaultValue)
        {
            try
            {
                return string.IsNullOrEmpty(dateString) ? defaultValue : Convert.ToDateTime(dateString);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion string parsing  //--------------------------------------------------------------------


        #region number formatting  //--------------------------------------------------------------------
        /// <summary>
        /// specifies the format to use when converting a number to a display string
        /// </summary>
        public enum NumberFormat
        {
            /// <summary>
            /// fixed point, show thousand separators: -1,234,567.123
            /// this is the default
            /// NumDecimals, optional, default: 0
            /// MinTotalWidth, optional, default: 0
            /// </summary>
            Normal,

            /// <summary>
            /// fixed point, no thousand seperator: -1234567.123
            /// NumDecimals, optional, default: 0
            /// MinTotalWidth, optional, default: 0
            /// </summary>
            NoSeparators,

            /// <summary>
            /// most compact form of fixed-point (no separator) or scientific notation: -12345, -1e7
            /// MinPrecision, optional, default: 3
            /// MinTotalWidth, optional, default: 0
            /// Details:
            ///   if num >= 1e6: Scientific with MinPrecision digits
            ///   if 1e-4<= num < 1e6: fixed point,
            ///                        show all digits before the point and as many digits after the point to satisfy MinPrecision.
            ///                        No trailing zero after the point.
            ///   if 0 < num < 1e-4 (more than 3 zeros after point): Scientific with MinPrecision digits
            ///   if num < 0: same as -num
            /// </summary>
            Compact,

            /// <summary>
            /// Multiply value by 100, add % label
            /// If total width is specified, reduce by 2
            /// NumDecimals, optional, default: 0
            /// MinTotalWidth, optional, default: 0
            /// </summary>
            Percent,

            /// <summary>
            /// Same as Percent but supresses separators
            /// NumDecimals, optional, default: 0
            /// MinTotalWidth, optional, default: 0
            /// </summary>
            PercentNoSeparators
        }

        /// <summary>
        /// converts double or float to string, with specified number of decimals and number format
        /// </summary>
        /// <param name="precision">Means numberOfDecimals for Normal, NoSeparators, Percent, PercentNoSeparators. Means minPrecision for Compact</param>
        static public string ToDisplayString(double val, int precision, NumberFormat format = NumberFormat.Normal)
        {
            switch (format)
            {
                case NumberFormat.Normal:
                    return val.ToString("n" + precision.ToString());
                case NumberFormat.NoSeparators:
                    return val.ToString("f" + precision.ToString());
                case NumberFormat.Compact:
                    var negative = false;
                    string result;
                    if (val < 0)
                    {
                        val = -val;
                        negative = true;
                    }
                    //if 1e-4 <= val < 1e6
                    // use normal format with precision = totalWidth - 1 - digits to left of decimal point
                    //but not less than zero
                    if (val < 1e6 && val >= 1e-4)
                    {
                        int digitsToLeft;
                        if (val >= 1.0F)
                        {
                            result = ((int)val).ToString("f0");
                            digitsToLeft = result.Length;
                            if (digitsToLeft >= precision)
                            {
                                precision = 0;
                                val = (double)((int)val);
                            }
                            else
                                precision -= digitsToLeft;
                            result = val.ToString("f" + precision.ToString());
                            StripTrailingDecimalZeros(ref result);
                        }
                        //val < 1.0F but >= 1e-4
                        else
                        {
                            result = val.ToString("f4");
                            digitsToLeft = 0;
                            while (result.Substring(digitsToLeft + 2, 1) == "0")
                                digitsToLeft++;
                            precision += digitsToLeft;
                            result = val.ToString("f" + precision.ToString());
                            StripTrailingDecimalZeros(ref result);
                        }
                    }
                    //else use scientific notation
                    else
                    {
                        //construct format #.#######e0
                        var formatString = "#";
                        if (precision > 0)
                        {
                            formatString += ".";
                            formatString = formatString.PadRight(precision + 1, '#');
                        }
                        formatString += "e0";
                        result = val.ToString(formatString);
                        if (result == "0e0")
                            result = "0";
                    }
                    if (negative)
                        result = "-" + result;
                    return result;
                case NumberFormat.Percent:
                    val *= 100;
                    return val.ToString("n" + precision.ToString()) + " %";
                case NumberFormat.PercentNoSeparators:
                    return val.ToString("f" + precision.ToString()) + " %";
                default:
                    throw new CyteException("CyteConvert", "Invalid NumberFormat");
            }
        }

        /// <summary>
        /// helper. Removes trailing decimals that are 0
        /// </summary>
        static private void StripTrailingDecimalZeros(ref string str)
        {
            if (str.IndexOf('.') >= 0) //check if there are decimals
            {
                while (str.Substring(str.Length - 1, 1) == "0")
                    str = str.Substring(0, str.Length - 1);
                if (str.Substring(str.Length - 1, 1) == ".")
                    str = str.Substring(0, str.Length - 1);
            }
        }

        /// <summary>
        /// converts double or float to string, with specified number of decimals / precision, number format, and total width
        /// </summary>
        /// <param name="val"></param>
        /// <param name="precision">Means numberOfDecimals for Normal, NoSeparators, Percent, PercentNoSeparators. Means minPrecision for Compact</param>
        /// <param name="format">number format see NumberFormat enum</param>
        /// <param name="totalWidth">resulting string will be at least this many characters wide, spaces will be prepended as needed</param>
        /// <returns></returns>
        static public string ToDisplayString(double val, int precision, NumberFormat format, int totalWidth)
        {
            var s = ToDisplayString(val, precision, format);
            if (format == NumberFormat.Percent)
                totalWidth -= 2;
            if (s.Length < totalWidth)
                s = s.PadLeft(totalWidth);
            return s;
        }

        /// <summary>
        /// converts double of flat to the format specified with default decimals and width
        /// according to format type
        /// </summary>
        /// <param name="val"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        static public string ToDisplayString(double val, NumberFormat format = NumberFormat.Normal)
        {
            // Switch used for future expansion, if would be tighter
            switch (format)
            {
                case NumberFormat.Compact:
                    return ToDisplayString(val, 3, format, 0);	//Compact get minPrecision=3, minWidth=0
                case NumberFormat.Normal:
                    return ToDisplayString(val, 0, format, 0);	//Normal get numberOfDecimals=0, minWidth=0
                case NumberFormat.NoSeparators:
                    return ToDisplayString(val, 0, format, 0);	//NoSeparators get numberOfDecimals=0, minWidth=0
                case NumberFormat.Percent:
                    return ToDisplayString(val, 0, format, 0);	//Percent get numberOfDecimals=0, minWidth=0
                case NumberFormat.PercentNoSeparators:
                    return ToDisplayString(val, 0, format, 0);	//PercentNoSeparators get numberOfDecimals=0, minWidth=0
                default:
                    throw new CyteException("CyteConvert", "Invalid NumberFormat");
            }
        }

        /// <summary>
        /// converts int to string, normal format
        /// example: 1,234
        /// </summary>
        static public string ToDisplayString(int val)
        {
            return ToDisplayString((double)val, NumberFormat.Normal);
        }

        /// <summary>
        /// converts int to string, with specified number format
        /// </summary>
        static public string ToDisplayString(int val, NumberFormat format)
        {
            return ToDisplayString((double)val, format);
        }

        /// <summary>
        /// converts int to string, with specified number format and total width
        /// </summary>
        static public string ToDisplayString(int val, NumberFormat format, int totalWidth)
        {
            // Switch used for future expansion, if would be tighter
            switch (format)
            {
                case NumberFormat.Compact:
                    return ToDisplayString((double)val, 3, format, totalWidth);	//Compact get minPrecision=3
                case NumberFormat.Normal:
                    return ToDisplayString((double)val, 0, format, totalWidth);	//Normal get numberOfDecimals=0
                case NumberFormat.NoSeparators:
                    return ToDisplayString((double)val, 0, format, totalWidth);	//NoSeparators get numberOfDecimals=0
                case NumberFormat.Percent:
                    return ToDisplayString((double)val, 0, format, totalWidth);	//Percent get numberOfDecimals=0
                case NumberFormat.PercentNoSeparators:
                    return ToDisplayString((double)val, 0, format, totalWidth);	//PercentNoSeparators get numberOfDecimals=0
                default:
                    throw new CyteException("CyteConvert", "Invalid NumberFormat");
            }
        }

        #endregion number formatting  //--------------------------------------------------------------------


        #region time formatting  //--------------------------------------------------------------------
        /// <summary>
        /// converts time to HH:MM:SS string
        /// </summary>
        static public string TimeToString(long ms)
        {
            var ts = new TimeSpan(ms * 10000);
            var hours = ts.Days * 24 + ts.Hours;
            return hours == 0 ? string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds) : string.Format("{0:00}:{1:00}:{2:00}", hours, ts.Minutes, ts.Seconds);
        }

        /// <summary>
        /// converts to date time format for saving
        /// </summary>
        static public string DateTimeToFileString(DateTime dt)
        {
            var temp = dt.ToString("u"); //returns "2004-03-11 16:01:25Z"
            return temp.Substring(0, temp.Length - 1); //without "Z"
        }

        /// <summary>
        /// converts to date time format for displaying to user
        /// uses current culture
        /// </summary>
        public enum DateTimeFormat
        {
            DateOnly,
            DateTimeNoSeconds,
            DateTimeWithSeconds,
            TimeOnlyNoSeconds,
            TimeOnlyWithSeconds,
        };

        static public string DateTimeToDisplayString(DateTime dt, DateTimeFormat dtf)
        {
            switch (dtf)
            {
                case DateTimeFormat.DateOnly:
                    return dt.ToString("d");
                case DateTimeFormat.DateTimeNoSeconds:
                    return dt.ToString("g");
                case DateTimeFormat.TimeOnlyNoSeconds:
                    return dt.ToString("t");
                case DateTimeFormat.TimeOnlyWithSeconds:
                    return dt.ToString("T");

                case DateTimeFormat.DateTimeWithSeconds:
                default:
                    return dt.ToString("G");
            }
        }

        #endregion time formatting  //--------------------------------------------------------------------


    }
}
