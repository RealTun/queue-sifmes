using QueueSifmes.Helpers;
using QueueSifmes.Services;
using QueueSifmes.StationDataPLC;
using S7.Net;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace QueueSifmes
{
    public class StationService
    {
        private readonly StationServiceManager stationManager;
        private Plc plcClient;
        private readonly ConcurrentQueue<object> stationQueue;
        private Thread processingThread;
        private bool isRunning;
        private int plcDB;
        private int plcStation;
        private string ip;

        public StationService(string ip, int plcStation, StationServiceManager stationManager)
        {
            //plcClient = new Plc(CpuType.S71500, ip, 0, 1);
            //plcClient.ReadTimeout = 5000;
            //plcClient.WriteTimeout = 5000;
            plcDB = 10;
            this.stationManager = stationManager; // Gán giá trị
            this.ip = ip;
            this.plcStation = plcStation;

            stationQueue = new ConcurrentQueue<object>();
            isRunning = true;

            processingThread = new Thread(ProcessQueue) { IsBackground = true };
            processingThread.Start();
        }

        // Mở kết nối PLC
        private void OpenConnection()
        {
            if (!plcClient.IsConnected)
            {
                plcClient.Open();
            }
        }

        // Đóng kết nối PLC
        private void CloseConnection()
        {
            if (plcClient.IsConnected)
            {
                plcClient.Close();
            }
        }

        #region process queue
        public void EnqueueStation(object data)
        {
            stationQueue.Enqueue(data);
        }

        private async void ProcessQueue()
        {
            while (isRunning)
            {
                if (stationQueue.TryDequeue(out object data))
                {
                    try
                    {
                        //OpenConnection();
                        if (data is StationData stationData)
                        {
                            //if (plcStation == 401)
                            //{
                            //    ProcessStation401(stationData);
                            //}
                            //else if (plcStation == 402)
                            //{
                            //    ProcessStation402(stationData);
                            //}
                            //else if (plcStation == 405)
                            //{
                            //    ProcessStation405();
                            //}
                            //else if (plcStation == 407)
                            //{
                            //    ProcessStation407();
                            //}

                            Console.WriteLine($"Station {plcStation} processing index: {stationData.CurrentIndexContainer}");

                            await HookService.SendDataAsync(new { station = plcStation, index = stationData.CurrentIndexContainer, status = 0 });

                            // mô phỏng thời gian xử lý
                            await Task.Delay(1000);

                            await HookService.SendDataAsync(new { station = plcStation, index = stationData.CurrentIndexContainer, status = 1 });

                            // mô phỏng thời gian chuyển tới trạm tiếp theo
                            await Task.Delay(2000);
                            //WaitForCompletionAsync();

                            // Chuyển phần tử tới station tiếp theo
                            string nextStationIp = GetNextStationIp();
                            if (!string.IsNullOrEmpty(nextStationIp))
                            {
                                stationManager.EnqueueStation(nextStationIp, stationData);
                                Console.WriteLine($"Data forwarded to Station {nextStationIp} for index: {stationData.CurrentIndexContainer}");
                            }
                            else
                            {
                                Console.WriteLine($"Station {plcStation} completed processing for index: {stationData.CurrentIndexContainer}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"Error processing SIF {plcClient?.IP ?? "N/A"}: {ex.Message}");
                        Console.WriteLine($"Error processing SIF {plcStation}: {ex.Message}");
                        if (plcStation == 407)
                        {
                            FileHelper.DeleteFile();
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private string GetNextStationIp()
        {
            if (plcStation == 401)
                return "192.168.0.1"; // IP của Station 402
            else if (plcStation == 402)
                return "192.168.0.18"; // IP của Station 405
            else if (plcStation == 405)
                return "192.168.0.20"; // IP của Station 407
            else
                return null; // Không có station tiếp theo
        }
        #endregion

        #region read, reset plc

        private bool CheckAcknowledgment()
        {
            OpenConnection();
            return APIClient.read_bool(plcClient, plcDB, 1);
        }

        public void updateStatus()
        {
            ResetAllIntData();
            OpenConnection();
            APIClient.sendBool(plcClient, plcDB, 1, false);
        }

        private void ResetAllIntData()
        {
            // Số lượng biến INT cần reset
            const int NUMBER_OF_INTS = 50;

            // Reset từng biến INT một
            for (int i = 0; i < NUMBER_OF_INTS; i++)
            {
                APIClient.sendInt(plcClient, plcDB, i, 0);
            }

            // Đóng kết nối
            CloseConnection();
        }

        public void Stop()
        {
            ResetPLC();
            isRunning = false;
            processingThread.Join();
            CloseConnection();
        }

        public void ResetPLC()
        {
            APIClient.sendString(plcClient, plcDB, start_byte_for_struct.string_start_byte, "");
            APIClient.sendInt(plcClient, plcDB, 0, 0);
            APIClient.sendInt(plcClient, plcDB, 1, 0);
            APIClient.sendInt(plcClient, plcDB, 2, 0);
            APIClient.sendBool(plcClient, plcDB, 1, false);
        }

        #endregion

        #region process station

        private void ProcessStation401(StationData stationData)
        {
            //APIClient.sendString(plcClient, plcDB, start_byte_for_struct.string_start_byte, stationData.RFID);
            while (true)
            {
                bool flag = CheckAcknowledgment();
                if (flag == true)
                {
                    break;
                }
            }
            updateStatus();
        }

        private void ProcessStation402(StationData stationData)
        {
            APIClient.sendInt(plcClient, plcDB, 0, stationData.Material_Id);
            APIClient.sendInt(plcClient, plcDB, 1, stationData.Quantity);
            while (true)
            {
                bool flag = CheckAcknowledgment();
                if (flag == true)
                {
                    break;
                }
            }
            updateStatus();
        }

        private async void ProcessStation405()
        {
            APIClient.sendBool(plcClient, plcDB, 2, true);
            while (true)
            {
                bool bool3Status = APIClient.read_bool(plcClient, plcDB, 3);

                if (bool3Status == true)
                {
                    string imagePath = "D:\\Work\\Report\\job69\\images\\nap.jpg";
                    bool lidVerificationResult = await DetectionService.DetectImage(imagePath); // module dung
                    if (lidVerificationResult == true)
                    {
                        // Nếu đã cấp nắp thành công
                        Console.WriteLine("Co nap");
                        APIClient.sendBool(plcClient, plcDB, 1, true);
                        APIClient.sendBool(plcClient, plcDB, 4, true);
                    }
                    else
                    {
                        // Nếu chưa cấp nắp
                        Console.WriteLine("Khong nap");
                        APIClient.sendBool(plcClient, plcDB, 1, true);
                        APIClient.sendBool(plcClient, plcDB, 4, false);
                    }

                    bool bool4Status = APIClient.read_bool(plcClient, plcDB, 4);
                    if (bool4Status == true)
                    {
                        // Nếu Bool4 = true, reset tất cả các bool và kết thúc xử lý thùng hiện tại
                        for (int i = 1; i <= 4; i++)
                        {
                            APIClient.sendBool(plcClient, plcDB, i, false);
                        }
                        break; // Xử lý thùng tiếp theo
                    }
                    else
                    {
                        APIClient.sendBool(plcClient, plcDB, 1, false);
                        APIClient.sendBool(plcClient, plcDB, 3, false);
                    }
                }
            }
        }

        private void ProcessStation407()
        {
            APIClient.sendBool(plcClient, plcDB, 2, true);
            while (true)
            {
                bool isLabeling = APIClient.read_bool(plcClient, plcDB, 3);
                if (isLabeling == true)
                {
                    bool isValid = true;
                    if (isValid)
                    {
                        APIClient.sendBool(plcClient, plcDB, 4, true);
                        bool result = CheckAcknowledgment();
                        if (result == true)
                        {
                            break;
                        }
                    }
                }
            }
            updateStatus();
        }

        #endregion
    }
}
