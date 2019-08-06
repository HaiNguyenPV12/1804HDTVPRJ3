using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class Dropdowns
    {
        public static IEnumerable<Airline> airlineDB = AirlineDAO.GetAirlineList();
        public static IEnumerable<Aircraft> aircraftDB = AircraftDAO.GetAircraftList();
        public static IEnumerable<Airport> airportDB = AirportDAO.GetAirlineList();
        public static IEnumerable<Route> routetDB = RouteDAO.GetRouteList();
        public static IEnumerable<SelectListItem> Airlines()
        {
            var list = new List<SelectListItem>();
            foreach (var item in airlineDB)
            {
                var newItem = new SelectListItem() { Value = item.AirlineID, Text = item.AirlineName };
                list.Add(newItem);
            }
            return list;
        }

        public static IEnumerable<SelectListItem> Aircrafts()
        {
            var list = new List<SelectListItem>();
            foreach (var item in aircraftDB)
            {
                var newItem = new SelectListItem() { Value = item.AircraftID, Text = item.AircraftName };
                list.Add(newItem);
            }
            return list;
        }

        public static IEnumerable<SelectListItem> Airports()
        {
            var list = new List<SelectListItem>();
            foreach (var item in airportDB)
            {
                var newItem = new SelectListItem() { Value = item.AirportID, Text = item.AirportName + " (" + item.AirportID + ")" };
                list.Add(newItem);
            }
            return list;
        }

        public static IEnumerable<SelectListItem> Routes()
        {
            var list = new List<SelectListItem>();
            routetDB = RouteDAO.GetRouteList();
            foreach (var item in routetDB)
            {
                var newItem = new SelectListItem() { Value = item.RNo.ToString(), Text = string.Format("{0}-{1}: {2}({3}) -> {4}({5})", item.RAirline, item.RAircraft, item.DepartureAirport.AirportName, item.Departure, item.DestinationAirport.AirportName, item.Destination) };
                list.Add(newItem);
            }
            return list;
        }
    }
}