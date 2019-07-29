using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class FlightResult
    {
        public Flight FlightVM { get; set; }
        public Route RouteVM { get; set; }
        public Airline AirlineVM { get; set; }
    }
}