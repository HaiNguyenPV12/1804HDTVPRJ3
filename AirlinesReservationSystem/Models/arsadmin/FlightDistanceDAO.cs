using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class FlightDistanceDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static IEnumerable<FlightDistance> GetFlightDistances() => db.FlightDistance;

        public static FlightDistance GetFlightDistance(string airport1, string airport2)
        {
            db = new AirlineDBEntities();
            var d = GetFlightDistances().Where(item => item.AirportID1 == airport1 && item.AirportID2 == airport2).FirstOrDefault();
            if (d == null)
            {
                d = GetFlightDistances().Where(item => item.AirportID1 == airport2 && item.AirportID2 == airport1).FirstOrDefault();
            }
            return d;
        }
    }
}