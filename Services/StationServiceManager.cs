using QueueSifmes.Interface;
using QueueSifmes.StationDataPLC;
using System;
using System.Collections.Generic;

namespace QueueSifmes.Services
{
    public class StationServiceManager : IStationAPI
    {
        private readonly Dictionary<string, StationService> stationServices;

        public StationServiceManager()
        {
            stationServices = new Dictionary<string, StationService>();
        }

        // Khởi tạo một StationService mới
        public void AddStation(string ip, int station)
        {
            if (!stationServices.ContainsKey(ip))
            {
                var service = new StationService(ip, station, this); // Truyền `this` vào
                stationServices.Add(ip, service);
                Console.WriteLine($"StationService for {ip} added.");
            }
            else
            {
                Console.WriteLine($"StationService for {ip} already exists.");
            }
        }


        // Dừng và xóa một StationService
        public void RemoveStation(string ip)
        {
            if (stationServices.TryGetValue(ip, out var service))
            {
                service.Stop();
                stationServices.Remove(ip);
                Console.WriteLine($"StationService for {ip} removed.");
            }
            else
            {
                Console.WriteLine($"StationService for {ip} does not exist.");
            }
        }

        // Thêm giá trị vào hàng đợi của một StationService
        public void EnqueueStation(string ip, object data)
        {
            if (stationServices.TryGetValue(ip, out var service))
            {
                service.EnqueueStation(data);
            }
            else
            {
                Console.WriteLine($"StationService for {ip} not found.");
            }
        }

        // Dừng tất cả các StationService
        public void StopAll()
        {
            foreach (var service in stationServices.Values)
            {
                service.Stop();
            }
            stationServices.Clear();
            Console.WriteLine("All StationServices stopped.");
        }
    }
}
