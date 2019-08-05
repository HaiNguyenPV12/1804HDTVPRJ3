using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class ReScheduleDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static FlightSearch GetSearchParamByOrder(long orderid)
        {
            var order = db.Order.FirstOrDefault(o => o.OrderID == orderid);
            FlightSearch searchParams = new FlightSearch();
            if (order != null)
            {
                bool isReturn = false;
                var ticketList = db.Ticket.Where(t => t.OrderID == orderid);
                // Check if this have
                foreach (var ticket in ticketList)
                {
                    if (ticket.IsReturn)
                    {
                        isReturn = true;
                        break;
                    }
                }

            }
            return null;
        }
    }
}