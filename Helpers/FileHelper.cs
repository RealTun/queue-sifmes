using QueueSifmes.Models;
using QueueSifmes.StationDataPLC;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace QueueSifmes.Helpers
{
    internal class FileHelper
    {
        //private static string outputFile = @"D:\Workspace\sifmes_ngangiang_web\public\storage\output.txt";
        //private static string ipFile = @"D:\Workspace\sifmes_ngangiang_web\public\storage\outputIP.txt";

        private static string outputFile = @"D:\Work\Report\job69\output.txt";
        private static string ipFile = @"D:\Work\Report\job69\outputIP.txt";
        public static bool IsExistFile()
        {
            return File.Exists(outputFile);
        }

        public static void DeleteFile()
        {
            File.Delete(outputFile);
        }
        public static StationData ReadFile()
        {
            string content = File.ReadAllText(outputFile);
            var data = ReadLine(content);
            return data;
        }
        public static List<IPData> ReadAllLines()
        {
            List<IPData> list = new List<IPData>();
            string[] lines = File.ReadAllLines(ipFile);
            foreach (string line in lines)
            {
                var data = getIPLine(line);
                if (data != null)
                {
                    list.Add(data);
                }
            }
            return list;
        }
        public static IPData getIPLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            string[] parts = line.Split('-');
            return new IPData
            {
                IdStation = int.Parse(parts[0]),
                IP = parts[1]
            };
        }
        private static StationData ReadLine(string line)
        {
            string[] parts = line.Split(new char[] { ' ' });
            return new StationData
            {
                Material_Id = int.Parse(parts[0]),
                Quantity = int.Parse(parts[1]),
                CountContainer = int.Parse(parts[2])
            };
        }

        public static void DeleteImagesInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Thư mục không tồn tại.");
                return;
            }

            var imageExtensions = new[] { ".jpg", ".png", ".jpeg", ".gif" };
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                if (Array.Exists(imageExtensions, ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    File.Delete(file);
                    Console.WriteLine($"Đã xóa: {file}");
                }
            }
            Console.WriteLine("Hoàn thành việc xóa ảnh.");
        }
    }
}
