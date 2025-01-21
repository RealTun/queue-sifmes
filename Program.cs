using QueueSifmes.Helpers;
using QueueSifmes.Models;
using QueueSifmes.Services;
using QueueSifmes.StationDataPLC;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace QueueSifmes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var stationServiceManager = new StationServiceManager();

            // params
            if (args.Length > 0)
            {
                string dataPath = args[0];
                string ipPath = args[1];

                FileHelper.SetDataPath(dataPath);
                FileHelper.SetIpPath(ipPath);
            }  

            List<IPData> listData = FileHelper.ReadAllLines();

            // check connection
            Console.WriteLine("Checking connection!");

            foreach (IPData item in listData)
            {
                try
                {
                    Plc plcClient = new Plc(CpuType.S71500, item.IP, 0, 1);
                    plcClient.Open();

                    if (plcClient.IsConnected)
                    {
                        //Console.WriteLine($"Connected to {item.IP}");
                        stationServiceManager.AddStation(item.IP, item.IdStation);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error connecting to {item.IP}: {ex.Message}");
                }
            }

            Console.WriteLine("\nPress eny button to start process station!");
            Console.ReadKey();

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
