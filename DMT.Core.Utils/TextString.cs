using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DMT.Core.Utils
{
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

    }
}
