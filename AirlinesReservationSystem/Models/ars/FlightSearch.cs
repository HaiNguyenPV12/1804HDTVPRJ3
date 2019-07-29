using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class FlightSearch
    {
        [Required]
        public string Departure { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        public DateTime ReturnDepartureTime { get; set; }

        public int Adult { get; set; }

        public int Children { get; set; }

        public int Senior { get; set; }

        public bool IsRoundTrip { get; set; }

        [Required]
        public string Class { get; set; }
    }
}