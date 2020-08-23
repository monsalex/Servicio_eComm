using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WServiceComm
{
    public class ShipmentTracking
    {
        public int Id { get; set; }
        public string OrdenId { get; set; }
        public string status { get; set; }
        public string enviaya_shipment_number { get; set; }
        public string carrier_tracking_number { get; set; }
        public DateTime estimated_delivery_date { get; set; }
        public DateTime expected_delivery_date { get; set; }
        public DateTime delivery_date { get; set; }
        public DateTime pickup_date { get; set; }
        public string shipment_status { get; set; }
        public Int32? event_code { get; set; }
        public string event_description { get; set; }
        public string event_ { get; set; }
        public Int32? status_code { get; set; }
        public string sub_event_code { get; set; }
        public string sub_event { get; set; }
        public string sub_event_description { get; set; }
    }

    public class EnviaYa
    {
        public string OrdenId { get; set; }
        public string carrier_account { get; set; }
        public string api_key { get; set; }
        public string carrier { get; set; }
        public string shipment_number { get; set; }
    }
}
