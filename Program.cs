using QueueSifmes.Helpers;
using QueueSifmes.Services;
using QueueSifmes.StationDataPLC;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueSifmes
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string ip1 = "192.168.1.15";
            const string ip2 = "192.168.0.1";
            const string ip3 = "192.168.0.18";
            const string ip4 = "192.168.0.20";

            var stationServiceManager = new StationServiceManager();

            // Khởi tạo các station
            stationServiceManager.AddStation("192.168.1.15", 401); // Station 401
            stationServiceManager.AddStation("192.168.0.1", 402); // Station 402
            stationServiceManager.AddStation("192.168.0.18", 405); // Station 405
            stationServiceManager.AddStation("192.168.0.20", 407); // Station 407

            while (true)
            {
                if (FileHelper.IsExistFile())
                {
                    Console.WriteLine("\nFile exists! Reading file...");
                    var stationData = FileHelper.ReadFile();

                    for (int i = 0; i < stationData.CountContainer; i++)
                    {
                        var data = new StationData
                        {
                            RFID = stationData.RFID,
                            Material_Id = stationData.Material_Id,
                            Quantity = stationData.Quantity,
                            CountContainer = stationData.CountContainer,
                            CurrentIndexContainer = i
                        };

                        //Console.WriteLine($"Enqueueing container {i} to Station {ip1}");
                        stationServiceManager.EnqueueStation(ip1, data);
                    }

                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            Console.WriteLine("Processing stations...");
            Console.ReadLine();

            //stationServiceManager.StopAll();
            Console.ReadKey();
        }
    }
}
