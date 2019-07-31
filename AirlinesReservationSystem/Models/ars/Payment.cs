using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class Passenger
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool Sex { get; set; }
        public int Age { get; set; }
        public string PassportNo { get; set; }
        //public string Class { get; set; }
        //public string ReClass { get; set; }
        public string[] Service { get; set; }

    }

    public class Payment
    {
        public List<Passenger> Passengers { get; set; }
        public int PeopleNum { get; set; }
        public int AdultNum { get; set; }
        public int ChildNum { get; set; }
        public string FNo1 { get; set; }
        public string FNo2 { get; set; }
        public string ReFNo { get; set; }
        public string UserID { get; set; }
        public string Class { get; set; }
    }
}