using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class InsertDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        static IEnumerable<FlightDistance> distanceDB = db.FlightDistance;
        static IEnumerable<Airline> airlineDB = db.Airline;
        static IEnumerable<Aircraft> aircraftDB = db.Aircraft;

        public static void InsertRoutes()
        {
            foreach (var distance in distanceDB)
            {
                var r = new Route();
                var r2 = new Route();
                var randgen = new Random();

                r.RAircraft = randomAircraft(distance.Distance).AircraftID.Trim();
                r.RAirline = randomAirline().AirlineID.Trim();

                r.Departure = distance.AirportID1.Trim();
                r.Destination = distance.AirportID2.Trim();

                RouteDAO.AddRoute(r);

                r2.RAircraft = randomAircraft(distance.Distance).AircraftID.Trim();
                r2.RAirline = randomAirline().AirlineID.Trim();

                r2.Departure = distance.AirportID2.Trim();
                r2.Destination = distance.AirportID1.Trim();
                RouteDAO.AddRoute(r2);
            }
        }

        static Aircraft randomAircraft(int distance)
        {
            var randgen = new Random();
            var aircraft = aircraftDB.ElementAt(randgen.Next(0, aircraftDB.Count()));
            while (aircraft.Range < distance)
            {
                aircraft = aircraftDB.ElementAt(randgen.Next(0, aircraftDB.Count()));
            }
            return aircraft;
        }

        static Airline randomAirline()
        {
            var randgen = new Random();
            var airline = airlineDB.ElementAt(randgen.Next(0, airlineDB.Count()));
            return airline;
        }
    }
}