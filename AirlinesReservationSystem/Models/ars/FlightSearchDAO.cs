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
        private static IEnumerable<FlightResult> secondTripFromStop;
        public static IEnumerable<FlightResult> SecondTripFromStop { get => secondTripFromStop; set => secondTripFromStop = value; }

        public static IEnumerable<Flight> GetFlights() => db.Flight;

        public static IEnumerable<Route> GetRoutes() => db.Route;

        public static IEnumerable<FlightResult> GetFlightResults(FlightSearch flightSearch)
        {
            db = new AirlineDBEntities();
            var routes = GetRoutes();
            var flights = GetFlights();
            var airlines = GetAirlines();
            var model = from r in routes
                        join f in flights on r.RNo equals f.RNo
                        where r.Departure == flightSearch.Departure && r.Destination == flightSearch.Destination && f.DepartureTime.Date == flightSearch.DepartureTime.Date
                        select new FlightResult { FlightVM = f, RouteVM = r };
            return model;
        }

        public static IEnumerable<FlightResult> GetFlightResultsWithStops(FlightSearch flightSearch)
        {
            db = new AirlineDBEntities();
            var routes = GetRoutes();
            var flights = GetFlights();
            IEnumerable<FlightDistance> distances = GetDistances();
            IEnumerable<FlightDistance> trips = from d in distances
                                                where d.Distance <= 1000 && (d.AirportID1 == flightSearch.Destination || d.AirportID2 == flightSearch.Destination)
                                                select d;

            List<FlightResult> secondFlights = new List<FlightResult>();
            List<FlightResult> firstFlights = new List<FlightResult>();

            foreach (var item in trips)
            {
                //move destinations to 2nd collumn for easier sorting
                if (item.AirportID1 == flightSearch.Destination)
                {
                    var a2 = item.AirportID2;
                    var a1 = item.AirportID1;
                    item.AirportID1 = a2;
                    item.AirportID2 = a1;
                }

                var secondFlightToAdd = from r in routes
                                        join f in flights on r.RNo equals f.RNo
                                        where r.Departure == item.AirportID1 && r.Destination == item.AirportID2 && f.DepartureTime.Date >= flightSearch.DepartureTime.Date
                                        select new FlightResult { FlightVM = f, RouteVM = r };

                //Add to results for 2nd trip
                foreach (var f in secondFlightToAdd)
                {
                    if (f != null)
                    {
                        secondFlights.Add(f);
                    }
                }
            }

            //look for trips to second trip's starting point
            foreach (var item in secondFlights)
            {
                var firstFlightToAdd = from r in routes
                                       join f in flights on r.RNo equals f.RNo
                                       where r.Departure == flightSearch.Departure && r.Destination == item.RouteVM.Departure && f.DepartureTime.Date == flightSearch.DepartureTime.Date
                                       select new FlightResult { FlightVM = f, RouteVM = r };
                foreach (var f in firstFlightToAdd)
                {
                    //check if flight already exists
                    bool containsFlight = firstFlights.Any(i => i.FlightVM.FNo == f.FlightVM.FNo);
                    if (!containsFlight)
                    {
                        firstFlights.Add(f);
                    }
                }
            }
            secondTripFromStop = secondFlights;
            return firstFlights;
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

        public static IEnumerable<FlightDistance> GetDistances() => db.FlightDistance;

        public static IEnumerable<Airport> GetAirports() => db.Airport;

        public static IEnumerable<Airline> GetAirlines() => db.Airline;

        public static string GetAirlineIcon(string airlineID)
        {
            var airlines = GetAirlines();
            var airline = airlines.Where(item => item.AirlineID == airlineID).FirstOrDefault();
            var icon = airline.AirlineIcon;
            return icon;
        }

        //Create new search parameters in memory to manipulate
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