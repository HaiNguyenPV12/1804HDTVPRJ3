using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class AirportDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();
        public static IEnumerable<Airport> GetAirlineList() {
            db = new AirlineDBEntities();
            return db.Airport;
        }
    }
}