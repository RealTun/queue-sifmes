using QueueSifmes.Helpers;
using QueueSifmes.Models;
using QueueSifmes.Services;
using QueueSifmes.StationDataPLC;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QueueSifmes
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //try
            //{
            //    Plc plcClient = new Plc(CpuType.S71500, "192.168.2.40", 0, 1);
            //    plcClient.ReadTimeout = 5000;
            //    plcClient.WriteTimeout = 5000;
            //    plcClient.Open();

            //    if (plcClient.IsConnected)
            //    {
            //        Console.WriteLine("hello");
            //    }
            //    //Console.WriteLine("Good bai");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine($"loi: {e.Message}");
            //}

            var stationServiceManager = new StationServiceManager();

            List<IPData> listData = FileHelper.ReadAllLines();
            foreach (IPData each in listData)
            {
                stationServiceManager.AddStation(each.IP, each.IdStation); // Station 401
            }

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
                            Material_Id = stationData.Material_Id,
                            Quantity = stationData.Quantity,
                            CountContainer = stationData.CountContainer,
                            CurrentIndexContainer = i
                        };

                        //Console.WriteLine($"Enqueueing container {i} to Station {ip1}");
                        stationServiceManager.EnqueueStation(listData[0].IP, data);
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
