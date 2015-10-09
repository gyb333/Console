using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCore
{
    public class Test
    {
        public static void SplitToDic()
        {
            //netsh winsock reset命令

            //string strTemp = "000010:A;000020:D;000030:M";
            //string[] args = strTemp.Split(';');
           //var dic = strTemp.ToDictionary(':',';');
           //DataTable dt = new DataTable();
           //dt.Columns.Add("name",typeof(string));
           //dt.Columns.Add("IsDelete", typeof(int));

           //DataRow dr = dt.NewRow();
           //dt.Rows.Add(dr);
           //dr["name"] = "daf";
           //dr["IsDelete"] = 0;
           //if (dt.AsEnumerable().Where(p => p.Field<int>("IsDelete") == 0).Any())
           //{
           //}

            string s = @"/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJ\r\nChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/\r\n2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgo\r\nKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAFAASwDASIAAhEBAxEB/8QA\r\nHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUF\r\nBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkK\r\nFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1\r\ndnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXG\r\nx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEB\r\nAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAEC\r\nAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRom\r\nJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOE\r\nhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU\r\n1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDHmRlkc4Iy\r\nATVYxA/M+BjBrTbJO5v7uQaoSKcnI71sc4WemXN9bySQKWRMDpnmsS/s8syy\r\npgrxivYvhHqFvp29ZoN6XEvlEHnGR1rX+I/w1SdJb/Q4+QCzQgcj6Ur20YLX\r\nU+b59GtnRvlIYnmsefQ3BPksT7Gu0ntzA8kbcFTgg1SljOwgYzTC5wtxZzQs\r\nPMHGcZr6R8eeBxN4fsXtoMyx20YZgOD8v/1hXit9AHiCYzlgB9Sa+xrqzElh\r\nFGuBiBQec8bSCD/n1ppgz4g1vSprG5ZXUhh2xWKzbGyfxr6U+KfgwSRT3tqF\r\n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\r\n2Q==";

            var decodedBytes = System.Convert.FromBase64String(s);


            Base64 b = new Base64(Base64.RFC4648_RADIX_STRING);
            var ii = b.decode(s);
            var temp = b.decodeBytes(ii);
            Image i = convertImg(temp);
            i.Save(@"C:\Users\zhongduzhi.WCS-GLOBAL\Desktop\研究学习");
            //args.ToDictionary((key,value)=>key.
            DataTable dt = new DataTable();
            

        }


        //二进制生成图片
        private static Image convertImg(byte[] datas)
        {
            MemoryStream ms = new MemoryStream(datas);
            Image img = Image.FromStream(ms, true);
            ms.Close();
            return img;
        }

        /// <summary>
        /// Inflater解压
        /// </summary>
        /// <param name="baseBytes"></param>
        /// <returns></returns>
        public static string Decompress(byte[] baseBytes)
        {
            string resultStr = string.Empty;
            //using (MemoryStream memoryStream = new MemoryStream(baseBytes))
            //{
            //    using (InflaterInputStream inf = new InflaterInputStream(memoryStream))
            //    {
            //        using (MemoryStream buffer = new MemoryStream())
            //        {
            //            byte[] result = new byte[1024];

            //            int resLen;
            //            while ((resLen = inf.Read(result, 0, result.Length)) > 0)
            //            {
            //                buffer.Write(result, 0, resLen);
            //            }
            //            resultStr = Encoding.Default.GetString(result);
            //        }
            //    }
            //}
            return resultStr;
        }

    }


    #region Test
    public class Base64
    {
        private const String STANDARD_FIRST_62_CHARS =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";

        public const String STANDARD_RADIX_STRING =
                STANDARD_FIRST_62_CHARS + "+/";

        public const String RFC4648_RADIX_STRING =
                STANDARD_FIRST_62_CHARS + "-_";

        private const char padding = '=';
        private char[] radix_chars;

        public Base64(String radix_string)
        {
            radix_chars = radix_string.ToCharArray();
        }

        public Base64()
            : this(STANDARD_RADIX_STRING)
        {
        }

        public String decode(String str)
        {
            String binary_str = "";
            char[] chars = str.ToCharArray();
            int i = 0;
            for (; i < chars.Length && chars[i] != padding; ++i)
            {
                int index = indexOf(chars[i]);
                String bin_of_index = DecimalToBinary(index, 6);
                binary_str = binary_str + bin_of_index;
            }
            binary_str = binary_str.Substring(0, binary_str.Length - (2 * (chars.Length - i)));
            str = "";
            for (i = 0; i < binary_str.Length; i = i + 8)
            {
                String bin_str = binary_str.Substring(i, 8);
                int char_point = Convert.ToInt16(bin_str, 2);
                str = str + ((char)char_point);
            }
            return str;
        }

        public String encode(String str)
        {
            // TODO
            return null;
        }

        private String DecimalToBinary(int value, int length)
        {
            String bin_string = Convert.ToString(value, 2);
            length = length - bin_string.Length;
            while (length > 0)
            {
                bin_string = "0" + bin_string;
                --length;
            }
            return bin_string;
        }

        private int indexOf(char c)
        {
            for (int index = 0; index < radix_chars.Length; ++index)
                if (c == radix_chars[index])
                    return index;
            return -1;
        }

        public byte[] decodeBytes(String str)
        {
            String binary_str = "";
            char[] chars = str.ToCharArray();
            int i = 0;
            for (; i < chars.Length && chars[i] != padding; ++i)
            {
                int index = indexOf(chars[i]);
                String bin_of_index = DecimalToBinary(index, 6);
                binary_str = binary_str + bin_of_index;
            }
            binary_str = binary_str.Substring(0, binary_str.Length - (2 * (chars.Length - i)));
            byte[] bytes = new byte[binary_str.Length / 8];
            for (i = 0; i < binary_str.Length; i = i + 8)
            {
                String bin_str = binary_str.Substring(i, 8);
                int char_point = Convert.ToInt16(bin_str, 2);
                bytes[i / 8] = (byte)char_point;
            }
            return bytes;
        }
    }
#endregion


//    客户端使用android，服务端用.net




}
