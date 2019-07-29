using AirlinesReservationSystem.Models.ars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models
{
    public class FlightSearchDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static IEnumerable<Flight> GetFlights() => db.Flight;

        public static IEnumerable<Route> GetRoutes() => db.Route;

        public static IEnumerable<FlightResult> GetFlightResults(FlightSearch flightSearch)
        {
            var routes = GetRoutes();
            var flights = GetFlights();
            var airlines = GetAirlines();
            var model = from r in routes
                        join f in flights on r.RNo equals f.RNo
                        join a in airlines on r.RAirline equals a.AirlineID
                        where r.Departure == flightSearch.Departure && r.Destination == flightSearch.Destination && f.DepartureTime.Date == flightSearch.DepartureTime.Date
                        select new FlightResult { FlightVM = f, RouteVM = r, AirlineVM = a };
            return model;
        }

        public static FlightResult GetFlightResult(string fid, int rid)
        {
            var routes = GetRoutes();
            var flights = GetFlights();
            var model = (from r in routes
                         join f in flights on r.RNo equals f.RNo
                         where r.RNo == rid && f.FNo == fid
                         select new FlightResult { FlightVM = f, RouteVM = r }).FirstOrDefault();
            return model;
        }

        public static IEnumerable<Airport> GetAirports() => db.Airport;

        public static IEnumerable<Airline> GetAirlines() => db.Airline;

        public static string GetAirlineIcon(string airlineID)
        {
            var airlines = GetAirlines();
            var airline = airlines.Where(item => item.AirlineID == airlineID).FirstOrDefault();
            var icon = airline.AirlineIcon;
            return icon;
        }

        public static FlightSearch Copy(FlightSearch flightSearchOriginal)
        {
            FlightSearch f = new FlightSearch()
            {
                Departure = flightSearchOriginal.Departure,
                Adult = flightSearchOriginal.Adult,
                Children = flightSearchOriginal.Children,
                Class = flightSearchOriginal.Class,
                DepartureTime = flightSearchOriginal.DepartureTime,
                Destination = flightSearchOriginal.Destination,
                IsRoundTrip = flightSearchOriginal.IsRoundTrip,
                ReturnDepartureTime = flightSearchOriginal.ReturnDepartureTime,
                Senior = flightSearchOriginal.Senior
            };
            return f;
        }
    }
}