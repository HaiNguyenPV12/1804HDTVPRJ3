using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class RList
    {
        public int RNo { get; set; }
        public int Count { get; set; }

    }
    public class ALList
    {
        public string AirlineID { get; set; }
        public int Count { get; set; }

    }
    public class StatisticDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static IEnumerable<RList> TravelRoutes()
        {
            db = new AirlineDBEntities();
            var context = db.Ticket.Where(t => t.Order.Status == 1).Join(db.Flight, t => t.FNo, f => f.FNo, (t, f) => new { t.TicketID, f.RNo });
            return context.GroupBy(c => c.RNo).Select(n => new RList { RNo = n.Key, Count = n.Count() }).OrderByDescending(r => r.Count);
        }
        public static IEnumerable<ALList> TravelAirlines()
        {
            db = new AirlineDBEntities();
            var context = db.Ticket.Where(t => t.Order.Status == 1).Join(db.Flight, t => t.FNo, f => f.FNo, (t, f) => new { t.TicketID, f.Route.RAirline });
            return context.GroupBy(c => c.RAirline).Select(n => new ALList { AirlineID = n.Key, Count = n.Count() }).OrderByDescending(a => a.Count);
        }
    }
}