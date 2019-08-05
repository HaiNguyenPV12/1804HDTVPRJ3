using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class ReScheduleDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Airport GetAirport(string AirportID)
        {
            db = new AirlineDBEntities();
            return db.Airport.FirstOrDefault(a => a.AirportID == AirportID);
        }
        public static Order GetOrder(long OrderID)
        {
            db = new AirlineDBEntities();
            return db.Order.FirstOrDefault(o => o.OrderID == OrderID);
        }

        public static IEnumerable<TicketService> GetServiceList(long TicketID)
        {
            db = new AirlineDBEntities();
            return db.TicketService.Where(s => s.TicketID == TicketID);
        }

        public static FlightSearch GetSearchParamByOrder(long orderid)
        {
            db = new AirlineDBEntities();
            string s = "";
            var order = db.Order.FirstOrDefault(o => o.OrderID == orderid);
            FlightSearch searchParams = new FlightSearch();
            if (order != null)
            {
                bool isRoundtrip = false;
                bool haveStops = false;

                IEnumerable<Ticket> ticketList = db.Ticket.Where(t => t.OrderID == orderid);
                // Check if this have return flight
                foreach (var ticket in ticketList)
                {
                    if (ticket.IsReturn)
                    {
                        s += "is round trip\n";
                        isRoundtrip = true;
                        searchParams.IsRoundTrip = true;
                        break;
                    }
                }

                // Check if this is round trip or not
                if (isRoundtrip)
                {
                    Flight firstFlight = db.Flight.FirstOrDefault(f => f.FNo == ticketList.FirstOrDefault(t => t.IsReturn == false).FNo);
                    Route firstRoute = db.Route.FirstOrDefault(r => r.RNo == firstFlight.RNo);
                    searchParams.Departure = firstRoute.Departure;
                    searchParams.Destination = firstRoute.Destination;
                    searchParams.Class = ticketList.FirstOrDefault(t => t.IsReturn == false).Class;
                    int child = 0;
                    int adult = 0;
                    foreach (var item in ticketList.Where(t => t.IsReturn == false))
                    {
                        if (item.Age >= 14)
                        {
                            adult++;
                        }
                        else
                        {
                            child++;
                        }
                    }
                    s += "Adult: " + adult;
                    s += "Child: " + child;

                    searchParams.Children = child;
                    searchParams.Adult = adult;
                }
                else
                {
                    // In case there's no return flight, continue to check if this have stops or not
                    var ticketGroupList = ticketList.Where(t => t.IsReturn == false).GroupBy(t => new { t.Firstname, t.Lastname, t.Age, t.PassportNo });
                    if (ticketGroupList.FirstOrDefault().Count() > 1)
                    {
                        s += "have stops\n";
                        haveStops = true;
                        Ticket firstTicket = ticketGroupList.FirstOrDefault().FirstOrDefault();
                        Ticket secondTicket = ticketGroupList.FirstOrDefault().LastOrDefault();
                        Flight firstStop = db.Flight.FirstOrDefault(f => f.FNo == firstTicket.FNo);
                        Flight secondStop = db.Flight.FirstOrDefault(f => f.FNo == secondTicket.FNo);
                        Route firstRoute = db.Route.FirstOrDefault(r => r.RNo == firstStop.RNo);
                        Route secondRoute = db.Route.FirstOrDefault(r => r.RNo == secondStop.RNo);
                        searchParams.Departure = firstRoute.Departure;
                        searchParams.Destination = secondRoute.Destination;
                        searchParams.Class = ticketList.FirstOrDefault(t => t.IsReturn == false).Class;
                        int child = 0;
                        int adult = 0;
                        foreach (var item in ticketGroupList)
                        {
                            if (item.FirstOrDefault().Age >= 14)
                            {
                                adult++;
                            }
                            else
                            {
                                child++;
                            }
                        }

                        searchParams.Children = child;
                        searchParams.Adult = adult;
                    }
                    else
                    {
                        Flight firstFlight = db.Flight.FirstOrDefault(f => f.FNo == ticketList.FirstOrDefault().FNo);
                        Route firstRoute = db.Route.FirstOrDefault(r => r.RNo == firstFlight.RNo);
                        searchParams.Departure = firstRoute.Departure;
                        searchParams.Destination = firstRoute.Destination;
                        searchParams.Class = ticketList.FirstOrDefault().Class;
                        int child = 0;
                        int adult = 0;
                        foreach (var item in ticketList.Where(t => t.IsReturn == false))
                        {
                            if (item.Age >= 14)
                            {
                                adult++;
                            }
                            else
                            {
                                child++;
                            }
                        }
                        s += "Adult: " + adult;
                        s += "Child: " + child;

                        searchParams.Children = child;
                        searchParams.Adult = adult;
                    }
                }
            }
            return searchParams;
        }

        static public string ProcessReschedule(Payment payment)
        {
            var s = PaymentDAO.CancelOrder(payment.OldOrderID);
            if (s != "ok")
            {
                return s;
            }
            s = PaymentDAO.ProcessPayment(payment, false);
            return s;
        }
    }
}