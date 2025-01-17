using QueueSifmes.Helpers;
using QueueSifmes.Models;
using QueueSifmes.Services;
using QueueSifmes.StationDataPLC;
using S7.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            plcClient = new Plc(CpuType.S71500, ip, 0, 1);
            plcClient.ReadTimeout = 5000;
            plcClient.WriteTimeout = 5000;
            plcDB = 10;
            this.stationManager = stationManager; // Gán giá trị
            this.ip = ip;
            this.plcStation = plcStation;

            stationQueue = new ConcurrentQueue<object>();
            isRunning = true;

            processingThread = new Thread(ProcessQueue) { IsBackground = true };
            processingThread.Start();
        }

        #region connect, close
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

        #endregion

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
                        OpenConnection();
                        if (data is StationData stationData)
                        {
                            Console.WriteLine($"Station {plcStation} processing index: {stationData.CurrentIndexContainer}");

                            await HookService.SendDataAsync(new { station = plcStation, index = stationData.CurrentIndexContainer, status = 0 });

                            switch (plcStation)
                            {
                                case 401:
                                    ProcessStation401(stationData);
                                    break;
                                case 402:
                                    ProcessStation402(stationData);
                                    break;
                                case 405:
                                    ProcessStation405();
                                    break;
                                case 407:
                                    ProcessStation407();
                                    break;
                                case 408:
                                    ProcessStation408();
                                    break;
                                case 409:
                                    ProcessStation409();
                                    break;
                                default:
                                    break;
                            }

                            // mô phỏng thời gian xử lý
                            //await Task.Delay(1000);

                            await HookService.SendDataAsync(new { station = plcStation, index = stationData.CurrentIndexContainer, status = 1 });

                            // mô phỏng thời gian chuyển tới trạm tiếp theo
                            //await Task.Delay(2000);
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
                                //if (plcStation == 409)
                                //{
                                //    FileHelper.DeleteFile();
                                //}
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"Error processing SIF {plcClient?.IP ?? "N/A"}: {ex.Message}");
                        Console.WriteLine($"Error processing SIF {plcStation}: {ex.Message}");
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
            List<IPData> listData = FileHelper.ReadAllLines();
            for (int i = 0; i < listData.Count; i++)
            {
                if (listData[i].IdStation == plcStation)
                {
                    return listData[i + 1].IP;
                }
            }
            return null;
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
            while (true)
            {
                bool flag = CheckAcknowledgment();
                if (flag == true)
                {
                    var rfid = APIClient.read_string(plcClient, plcDB, start_byte_for_struct.string_start_byte);
                    break;
                }
            }
            //updateStatus();
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
            //updateStatus();
        }

        private async void ProcessStation405()
        {
            int circleOrSquare = 5;
            APIClient.sendBool(plcClient, plcDB, 2, true);
            APIClient.sendBool(plcClient, plcDB, circleOrSquare, true);
            APIClient.sendInt(plcClient, plcDB, circleOrSquare == 5 ? 0 : 1, 6);
            while (true)
            {
                bool bool3Status = APIClient.read_bool(plcClient, plcDB, 3);

                if (bool3Status == true)
                {
                    string imagePath = "D:\\Work\\Report\\job69\\images\\nap.jpg";
                    //bool lidVerificationResult = await DetectionService.DetectImage(imagePath); // module dung
                    bool lidVerificationResult = true;  // module dung
                    if (lidVerificationResult == true)
                    {
                        // Nếu đã cấp nắp thành công
                        //Console.WriteLine("Co nap");
                        APIClient.sendBool(plcClient, plcDB, 1, true);
                        APIClient.sendBool(plcClient, plcDB, 4, true);
                    }
                    else
                    {
                         //Nếu chưa cấp nắp
                        //Console.WriteLine("Khong nap");
                        APIClient.sendBool(plcClient, plcDB, 1, true);
                        APIClient.sendBool(plcClient, plcDB, 4, false);
                    }

                    bool bool4Status = APIClient.read_bool(plcClient, plcDB, 4);
                    if (bool4Status == true)
                    {
                        APIClient.sendBool(plcClient, plcDB, 2, false);
                        APIClient.sendBool(plcClient, plcDB, 4, false);
                        APIClient.sendBool(plcClient, plcDB, circleOrSquare, false);
                        APIClient.sendInt(plcClient, plcDB, circleOrSquare == 5 ? 0 : 1, 0);
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
                //bool isLabeling = APIClient.read_bool(plcClient, plcDB, 3);
                bool isLabeling = true;
                if (isLabeling == true)
                {
                    bool isValid = true;
                    if (isValid)
                    {
                        APIClient.sendBool(plcClient, plcDB, 4, true);
                        bool result = CheckAcknowledgment();
                        if (result == true)
                        {
                            APIClient.sendBool(plcClient, plcDB, 2, false);
                            APIClient.sendBool(plcClient, plcDB, 4, false);
                            break; // Xử lý thùng tiếp theo
                        }
                    }
                }
            }
            //updateStatus();
        }

        private void ProcessStation408()
        {
            APIClient.sendBool(plcClient, plcDB, 2, true);
            while (true)
            {
                bool result = CheckAcknowledgment();
                if (result == true)
                {
                    APIClient.sendBool(plcClient, plcDB, 2, false);
                    break;
                }
            }
            //updateStatus();
        }

        private void ProcessStation409()
        {
            APIClient.sendBool(plcClient, plcDB, 2, true);
            while (true)
            {
                bool result = CheckAcknowledgment();
                if (result == true)
                {
                    APIClient.sendBool(plcClient, plcDB, 2, false);
                    break;
                }
            }
        }

        #endregion
    }
}
