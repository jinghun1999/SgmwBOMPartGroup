using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SBS
{
    public static class ConvertUtility
    {
        public static int? ToInt32(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is bool)
            {
                return (bool)obj ? 1 : 0;
            }

            int result;
            if (!int.TryParse(obj.ToString().Trim(), out result))
            {
                return null;
            }
            return result;
        }
        public static short? ToInt16(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            short result;
            if (!short.TryParse(obj.ToString().Trim(), out result))
            {
                return null;
            }
            return result;
        }
        public static int ToInt32(object obj, int defaultValue)
        {
            int? result = ToInt32(obj);
            return result.HasValue ? result.Value : defaultValue;
        }

        public static byte? ToByte(object obj, byte defaultValue)
        {
            byte? result = ToByte(obj);
            return result.HasValue ? result.Value : defaultValue;
        }
        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        public static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        public static byte? ToByte(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is bool)
            {
                return (byte)((bool)obj ? 1 : 0);
            }

            byte result;
            if (!byte.TryParse(obj.ToString().Trim(), out result))
            {
                return null;
            }
            return result;
        }


        public static long? ToInt64(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is bool)
            {
                return (bool)obj ? 1 : 0;
            }

            long result;
            if (!long.TryParse(obj.ToString(), out result))
            {
                return null;
            }
            return result;
        }
        public static long ToInt64(object obj, long defaultValue)
        {
            long? result = ToInt64(obj);
            return result.HasValue ? result.Value : defaultValue;
        }

        public static string ToString(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }
            return obj.ToString();
        }
        public static string ToStringEmpty(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return "";
            }
            return obj.ToString();
        }
        public static string ToString(object obj, string defaultValue)
        {
            string result = ToString(obj);
            return !string.IsNullOrEmpty(result) ? result : defaultValue;
        }

        public static bool? ToBool(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is int)
            {
                return (int)obj != 0;
            }

            bool result;
            if (!bool.TryParse(obj.ToString().Trim(), out result))
            {
                return false;
            }
            return result;
        }

        public static bool ToBool(object obj, bool defaultValue)
        {
            bool? result = ToBool(obj);
            return result.HasValue ? result.Value : defaultValue;
        }

        public static Guid? ToGuid(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is Guid)
            {
                return (Guid)obj;
            }

            try
            {
                return new Guid(obj.ToString());
            }
            catch { }
            return null;
        }

        public static double? ToDouble(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is double?)
            {
                return (double?)obj;
            }

            double result;
            if (!double.TryParse(obj.ToString().Trim(), out result))
            {
                return null;
            }
            return result;
        }
        public static double ToDouble(object obj, double defaultValue)
        {
            double? result = ToDouble(obj);
            return result.HasValue ? result.Value : defaultValue;
        }

        public static decimal? ToDecimal(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is decimal?)
            {
                return (decimal?)obj;
            }

            decimal result;
            if (!decimal.TryParse(obj.ToString().Trim(), out result))
            {
                return null;
            }
            return result;
        }
        public static decimal ToDecimal(object obj, decimal defaultValue)
        {
            decimal? result = ToDecimal(obj);
            return result.HasValue ? result.Value : defaultValue;
        }

        public static DateTime? ToDateTime(object obj)
        {
            if (obj == null || obj.Equals(DBNull.Value))
            {
                return null;
            }

            if (obj is DateTime? || obj is DateTime)
            {
                return (DateTime?)obj;
            }

            DateTime result;
            if (!DateTime.TryParse(obj.ToString().Trim(), out result))
            {
                return null;
            }
            return result;
        }

        public static string ToSizeString(int obj)
        {
            if (obj > 1024 * 1024)
            {
                return ((decimal)obj / (1024 * 1024)).ToString("0.##") + "MB";
            }
            else if (obj > 1024)
            {
                return ((decimal)obj / (1024)).ToString("0.##") + "KB";
            }
            return obj.ToString() + "Byte";
        }

        /// <summary>
        /// Hash an input string and return the hash as
        /// a 32 character hexadecimal string.
        /// 哈希输入字符串并返回一个32字符的十六进制字符串哈希
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            //创建一个在MD5CryptoServiceProvider对象的新实例。
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            //输入字符串转换为一个字节数组并计算哈希。
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            //创建一个新的StringBuilder收集字节，创建一个字符串。
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            //依次通过每个散列数据和格式每一个十六进制字符串一个字节
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            //返回十六进制字符串
            return sBuilder.ToString();
        }

        public static string GetSelectString(string value, string split)
        {
            StringBuilder sb = new StringBuilder();
            string[] values = value.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in values)
            {
                sb.AppendFormat("'{0}',", item);
            }

            return sb.ToString().TrimEnd(',');
        }
        public static string GetLocation(string province, string city, string district)
        {
            if (!string.IsNullOrWhiteSpace(province) && !string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(district))
            {
                return string.Format("{0} {1} {2}", province, city, district);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 判断IDataReader中是否存在某字段
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool ReaderExists(IDataReader dr, string columnName)
        {
            dr.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columnName + "'";
            return (dr.GetSchemaTable().DefaultView.Count > 0);
        }
        #region 生成打印文件
        /// <summary>
        /// 取模板
        /// </summary>
        /// <returns></returns>
        public static StringBuilder GetStringFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(System.IO.File.ReadAllText(filePath, Encoding.GetEncoding("GB2312")));
                return sb;
            }
            return new StringBuilder();
        }
        #endregion
    }
}
