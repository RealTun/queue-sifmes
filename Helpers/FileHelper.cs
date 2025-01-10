using QueueSifmes.StationDataPLC;
using System;
using System.IO;

namespace QueueSifmes.Helpers
{
    internal class FileHelper
    {
        private static string filePath = @"D:\Work\Report\job69\output.txt";
        public static bool IsExistFile()
        {
            return File.Exists(filePath);
        }

        public static void DeleteFile()
        {
            File.Delete(filePath);
        }

        public static StationData ReadFile()
        {
            string content = File.ReadAllText(filePath);
            var data = ReadLine(content);
            return data;
        }

        private static StationData ReadLine(string line)
        {
            string[] parts = line.Split(new char[] { ' ' });
            return new StationData { RFID = parts[0], Material_Id = int.Parse(parts[1]), Quantity = int.Parse(parts[2]), CountContainer = int.Parse(parts[3]) };
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
