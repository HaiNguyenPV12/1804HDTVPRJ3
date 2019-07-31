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
    }
}