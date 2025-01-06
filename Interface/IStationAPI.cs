using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSifmes.Interface
{
    internal interface IStationAPI
    {
        void AddStation(string ip, int station);

        void RemoveStation(string ip);

        void EnqueueStation(string ip, object data);

        void StopAll();
    }
}
