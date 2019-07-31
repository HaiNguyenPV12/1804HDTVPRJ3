using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class AircraftDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();
        public static IEnumerable<Aircraft> GetAircraftList() => db.Aircraft;
    }
}