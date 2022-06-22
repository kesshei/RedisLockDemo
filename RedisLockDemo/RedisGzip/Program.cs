using StackExchange.Redis;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RedisGzip
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "redis Gzip Demo by 蓝创精英团队";
            var connMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1");
            Console.WriteLine($"redis连接状态:{connMultiplexer.IsConnected}");
            var db = connMultiplexer.GetDatabase(1);//可以选择指定的db，0-15

            //String
            db.StringSet("str1", GzipCompress("123"));
            var stringValue = db.StringGet("str1");
            var data = GzipDecompress(stringValue);
            Console.WriteLine($"获取到string 类型的值:{data}");


            Console.WriteLine("redis gzip 案例!");
            Console.ReadLine();
        }
        /// <summary>
        /// gzip压缩
        /// </summary>
        static byte[] GzipCompress(string data)
        {
            using var gzipdata = new MemoryStream();
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            using var gs = new GZipStream(gzipdata, CompressionMode.Compress, true);
            ms.CopyTo(gs);
            var bytes = gzipdata.GetBuffer();
            return bytes;
        }
        /// <summary>
        /// gzip解压
        /// </summary>
        static string GzipDecompress(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }
            using MemoryStream ms = new(data);
            using GZipStream compressedzipStream = new(ms, CompressionMode.Decompress);
            using MemoryStream outBuffer = new();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                {
                    break;
                }
                else
                {
                    outBuffer.Write(block, 0, bytesRead);
                }
            }
            var bytes = outBuffer.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
