using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class Dropdowns
    {
        static IEnumerable<Airline> airlineDB = AirlineDAO.GetAirlineList();
        static IEnumerable<Aircraft> aircraftDB = AircraftDAO.GetAircraftList();
        static IEnumerable<Airport> airportDB = AirportDAO.GetAirlineList();
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
    }
}