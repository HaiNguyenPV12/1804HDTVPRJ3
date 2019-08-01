using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class FlightCalendarDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Airport GetAirport(string id) => db.Airport.FirstOrDefault(a => a.AirportID == id);

        public static IEnumerable<Flight> GetFlight(string Departure, string Destination)
        {
            return db.Flight.Where(f => f.Route.Departure == Departure && f.Route.Destination == Destination);
        }
    }
}