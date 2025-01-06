namespace QueueSifmes.StationDataPLC
{
    public class StationData
    {
        public string RFID { get; set; }
        public int Material_Id { get; set; }
        public int Quantity { get; set; }
        public int CountContainer { get; set; }
        public int CurrentIndexContainer { get; set; }
    }
}
