﻿using AirlinesReservationSystem.Models.ars;
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
            var model = from r in routes
                        join f in flights on r.RNo equals f.RNo
                        where r.Departure == flightSearch.Departure && r.Destination == flightSearch.Destination && f.DepartureTime.Date == flightSearch.DepartureTime.Date
                        select new FlightResult { FlightVM = f, RouteVM = r };
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
    }
}