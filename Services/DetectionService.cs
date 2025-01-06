using Newtonsoft.Json;
using QueueSifmes.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using Grpc.Core;

namespace QueueSifmes.Services
{
    public class DetectionService
    {
        private static string _rootDir;
        private static string _resourceDir;
        private static string _modelDir;
        private static string _capturedImagesDir;
        private static string _modelBoxLidPath;
        private static string grpcHost = "localhost:50051"; // Địa chỉ server gRPC (không cần http://)

        private static void InitializePath()
        {
            _rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..");
            _resourceDir = Path.Combine(_rootDir, "Resource");
            _modelDir = Path.Combine(_rootDir, "Models");
            _capturedImagesDir = Path.Combine(_resourceDir, "CapturedImages");
            _modelBoxLidPath = Path.Combine(_modelDir, "box_lid_model_2.pt");
        }

        private static void InitializeServerGrpc()
        {
            string pythonScriptPath = Path.Combine(_rootDir, "Helpers", "Python", "GRPCServer.py");
            PythonExcutor pythonExecutor = new PythonExcutor("python", pythonScriptPath);
            pythonExecutor.Execute("");
        }

        public static async Task<bool> DetectImage(string imagePath)
        {
            // Kiểm tra sự tồn tại của ảnh
            //if (!File.Exists(imagePath))
            //{
            //    Console.WriteLine("Ảnh không tồn tại: " + imagePath);
            //    return;
            //}

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            InitializePath();
            InitializeServerGrpc();

            var detections = await gRpc(imagePath);

            //stopwatch.Stop();

            //Console.WriteLine($"Thời gian xử lý: {stopwatch.Elapsed.TotalSeconds} giây");

            if (isWithLid(detections))
            {
                return true;
            }
            return false;
        }

        private static async Task<List<dynamic>> gRpc(string imagePath)
        {
            try
            {
                // Địa chỉ server gRPC
                var channel = new Channel(grpcHost, ChannelCredentials.Insecure);

                //Tạo client gRPC
               var client = new ImageTransfer.ImageTransferClient(channel);

                // Gửi yêu cầu tới server
                var request = new ImageRequest { Path = imagePath, ModelPath = _modelBoxLidPath };

                // Gửi yêu cầu với timeout
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30))) // Thiết lập timeout
                {
                    var response = await client.SendImageAsync(request, cancellationToken: cts.Token);

                    // Xử lý phản hồi
                    if (response.Success)
                    {
                        //Console.WriteLine("Message: " + response.Message);

                        // Phân tích JSON thành danh sách dynamic
                        List<dynamic> detections = JsonConvert.DeserializeObject<List<dynamic>>(response.Data);

                        // Trả về danh sách phát hiện
                        return detections;
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.Message);
                        return new List<dynamic>(); // Trả về danh sách rỗng nếu có lỗi
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<dynamic>(); // Trả về danh sách rỗng nếu có ngoại lệ
            }
        }

        private static bool isWithLid(dynamic boundingBoxes)
        {
            foreach (var box in boundingBoxes)
            {
                if (box.label.ToString() == "Lid")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
