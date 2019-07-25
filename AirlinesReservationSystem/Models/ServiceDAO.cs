using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models
{
    public class ServiceDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Service GetService(string ServiceID) => db.Service.FirstOrDefault(s => s.ServiceID == ServiceID);

        public static IEnumerable<Service> GetServiceList() => db.Service.OrderByDescending(s => s.IsServing);
    }
}