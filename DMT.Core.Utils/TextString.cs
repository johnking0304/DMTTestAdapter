using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;
using System.IO;

namespace DMT.Core.Utils
{

    public class DoubleUtils
    {
        // Fields 浮点型的误差
        private const double DOUBLE_DELTA = 1E-06;

        public static bool AreEqual(double value1, double value2)
        {
            return (value1 == value2)
                || Math.Abs(value1 - value2) < DOUBLE_DELTA;
        }

        public static bool GreaterThan(double value1, double value2)
        {
            return ((value1 > value2) && !AreEqual(value1, value2));
        }

        public static bool GreaterThanOrEqual(double value1, double value2)
        {
            return (value1 > value2) || AreEqual(value1, value2);
        }

        public static bool IsZero(double value)
        {
            return (Math.Abs(value) < DOUBLE_DELTA);
        }

        public static bool LessThan(double value1, double value2)
        {
            return ((value1 < value2) && !AreEqual(value1, value2));
        }

        public static bool LessThanOrEqual(double value1, double value2)
        {
            return (value1 < value2) || AreEqual(value1, value2);
        }
    }


    public static class EnumHelper
    {
        #region Description
        /// <summary>
        /// 获取枚举值的描述文本
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Description(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(
            typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
        #endregion
    }


    public static class Files
    {

        /// <summary>
        /// 读取文件中所有字符
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string LoadFromFile(string filename)
        {
            string content = "";
            if (System.IO.File.Exists(filename))
            {
                StreamReader reader = new StreamReader(filename, Encoding.UTF8);
                content = reader.ReadToEnd();
                reader.Close();
            }
            return content;
        }

        public static string[] GetAllFiles(string path, string pattern = "*")
        {
            string[] filedir = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            return filedir;

        }


        public static string InitializePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

    }
    public static class TextString
    {

        public static string StringReverse(string text)
        {
            char[] chars = text.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string ParseStringIn(string sourse, string startstr, string endstr)
         {
             Regex rg = new Regex("(?<=(" + startstr + "))[.\\s\\S]*?(?=(" + endstr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
             return rg.Match(sourse).Value;
         }


    }




    public static class ByteUtils
    {
        /// <summary>
        /// 计算一组二进制数据的累加和。
        /// </summary>
        /// <param name="bytes">等待计算的二进制数据。</param>
        public static byte CalculateAccumulateSum(byte[] bytes)
        {
            int sum = 0;
            foreach (byte value in bytes)
            {
                sum += value;
            }
            // 对 256 取余，获得 1 个字节的数据。
            return (byte)(sum % 0x100);
        }

        /// <summary>
        /// 判断输入的字符串是否是有效的十六进制数据。
        /// </summary>
        /// <param name="hexStr">等待判断的十六进制数据。</param>
        /// <returns>符合规范则返回 True，不符合则返回 False。</returns>
        public static bool IsIllegalHexadecimal(string hexStr)
        {
            var validStr = hexStr.Replace("-", string.Empty).Replace(" ", string.Empty);
            if (validStr.Length % 2 != 0) return false;
            if (string.IsNullOrEmpty(hexStr) || string.IsNullOrWhiteSpace(hexStr)) return false;

            return new Regex(@"[A-Fa-f0-9]+$").IsMatch(hexStr);
        }

        /// <summary>
        /// 将 16 进制的字符串转换为字节数组。
        /// </summary>
        /// <param name="hexStr">等待转换的 16 进制字符串。</param>
        /// <returns>转换成功的字节数组。</returns>
        public static byte[] HexStringToBytes(string hexStr)
        {
            // 处理干扰，例如空格和 '-' 符号。
            var str = hexStr.Replace("-", string.Empty).Replace(" ", string.Empty);

            return Enumerable.Range(0, str.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(str.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// BCD 码转换成 <see cref="double"/> 类型。
        /// </summary>
        /// <param name="sourceBytes">等待转换的 BCD 码数据。</param>
        /// <param name="precisionIndex">精度位置，用于指示小数点所在的索引。</param>
        /// <returns>转换成功的值。</returns>
        public static double BCDToDouble(byte[] sourceBytes, int precisionIndex)
        {
            var sb = new StringBuilder();

            var reverseBytes = sourceBytes.Reverse().ToArray();
            for (int index = 0; index < reverseBytes.Length; index++)
            {
                sb.Append(reverseBytes[index] >> 4 & 0xF);
                sb.Append(reverseBytes[index] & 0xF);
                if (index == precisionIndex - 1) sb.Append('.');
            }

            return Convert.ToDouble(sb.ToString());
        }


        public static ushort[] BytesToUshorts(byte[] src, bool reverse = false)
        {
            int len = src.Length;

            byte[] srcPlus = new byte[len + 1];
            src.CopyTo(srcPlus, 0);
            int count = len >> 1;

            if (len % 2 != 0)
            {
                count += 1;
            }

            ushort[] dest = new ushort[count];
            if (reverse)
            {
                for (int i = 0; i < count; i++)
                {
                    dest[i] = (ushort)(srcPlus[i * 2] << 8 | srcPlus[2 * i + 1] & 0xff);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dest[i] = (ushort)(srcPlus[i * 2] & 0xff | srcPlus[2 * i + 1] << 8);
                }
            }

            return dest;
        }

        public static byte[] UshortsToBytes(ushort[] src, bool reverse = false)
        {

            int count = src.Length;
            byte[] dest = new byte[count << 1];
            if (reverse)
            {
                for (int i = 0; i < count; i++)
                {
                    dest[i * 2] = (byte)(src[i] >> 8);
                    dest[i * 2 + 1] = (byte)(src[i] >> 0);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dest[i * 2] = (byte)(src[i] >> 0);
                    dest[i * 2 + 1] = (byte)(src[i] >> 8);
                }
            }
            return dest;
        }


        /// <summary>
        /// BCD 码转换成 <see cref="string"/> 类型。
        /// </summary>
        /// <param name="sourceBytes">等待转换的 BCD 码数据。</param>
        /// <returns>转换成功的值。</returns>
        public static string BCDToString(byte[] sourceBytes)
        {
            var sb = new StringBuilder();
            var reverseBytes = sourceBytes.Reverse().ToArray();

            for (int index = 0; index < reverseBytes.Length; index++)
            {
                sb.Append(reverseBytes[index] >> 4 & 0xF);
                sb.Append(reverseBytes[index] & 0xF);
            }

            return sb.ToString();
        }



        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="byteDatas"></param>
        /// <returns></returns>
        public static string BytesToHexString(byte[] byteDatas)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < byteDatas.Length; i++)
            {
                builder.Append(string.Format("{0:X2}", byteDatas[i]));
            }
            return builder.ToString();
        }



        public static bool GetBitValue(byte value, byte index)
        {
            return (value & (byte)Math.Pow(2, index)) > 0 ? true : false;
        }



        /// <summary>
        /// 获取Single类型数据
        /// </summary>
        /// <param name="src"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static float GetSingle(ushort[] src, int start)
        {
            ushort[] temp = new ushort[2];
            for (int i = 0; i < 2; i++)
            {
                temp[i] = src[i + start];
            }
            byte[] bytesTemp = Ushorts2Bytes(temp);
            float res = BitConverter.ToSingle(bytesTemp, 0);
            return res;
        }


        private static byte[] Ushorts2Bytes(ushort[] src, bool reverse = false)
        {

            int count = src.Length;
            byte[] dest = new byte[count << 1];
            if (reverse)
            {
                for (int i = 0; i < count; i++)
                {
                    dest[i * 2] = (byte)(src[i] >> 8);
                    dest[i * 2 + 1] = (byte)(src[i] >> 0);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dest[i * 2] = (byte)(src[i] >> 0);
                    dest[i * 2 + 1] = (byte)(src[i] >> 8);
                }
            }
            return dest;
        }



        public static byte SetBitValue(byte data, byte index, bool value)
        {
            byte temp = value ? (byte)1 : (byte)0;
            data |= (byte)(temp << index);
            return data;

        }

        public static ushort SetBitValue(ushort data, byte index, bool value)
        {
            byte[] bytes = ByteUtils.UshortsToBytes(new ushort[1] { data });

            if (index >= 0 && index <= 7)
            {
                byte temp = value ? (byte)1 : (byte)0;
                bytes[0] |= (byte)(temp << index);

            }
            else if (index >= 8 && index <= 15)
            {
                byte temp = value ? (byte)1 : (byte)0;
                bytes[1] |= (byte)(temp << (index - 8));
            }
            ushort[] ushorts = ByteUtils.BytesToUshorts(bytes);

            return ushorts[0];
        }

        public static bool GetBitValue(ushort value, byte index)
        {
            byte[] bytes = ByteUtils.UshortsToBytes(new ushort[1] { value });

            bool data = false;


            if (index >= 0 && index <= 7)
            {
                data = (bytes[0] & (byte)Math.Pow(2, index)) > 0 ? true : false;
            }
            else if (index >= 8 && index <= 15)
            {
                data = (bytes[1] & (byte)Math.Pow(2, index - 8)) > 0 ? true : false;
            }
            return data;
        }

    }
}
