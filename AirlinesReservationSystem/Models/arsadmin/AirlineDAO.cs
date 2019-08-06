using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class AirlineDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();
        public static IEnumerable<Airline> GetAirlineList() => db.Airline;
        public static Airline GetAirline(string AirlineID) => db.Airline.FirstOrDefault(a => a.AirlineID == AirlineID);
    }
}