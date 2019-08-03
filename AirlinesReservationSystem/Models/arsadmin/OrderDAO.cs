using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class OrderDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Order GetOrder(long orderID) => db.Order.FirstOrDefault(s => s.OrderID == orderID);

        public static IEnumerable<Order> GetOrderList() => db.Order.OrderByDescending(s => s.Status);

        public static string CancelOrder(long id)
        {
            var o = GetOrder(id);
            var card = bank.BankDAO.GetCreditCard(db.User.FirstOrDefault(u => u.UserID == o.UserID).CCNo);
            if (o.Status == 1)
            {
                if (card == null)
                {
                    return "Error: Credit Card is not valid.";
                }
            }
            if (o != null)
            {
                // Check if this order blocked or not
                bool IsBlocked = false;
                if (o.Status == 0)
                {
                    IsBlocked = true;
                }
                // Change status
                o.Status = 2;
                int totalDistance = 0;
                double refund = 0;
                var TicketList = TicketDAO.GetTicketList(o.OrderID);

                // If this order is not blocked one, make changes to skymiles
                if (!IsBlocked)
                {
                    // Calculate flight distance 
                    foreach (var item in TicketList)
                    {
                        var TInfo = TicketList.FirstOrDefault(t => t.TicketID == item.TicketID);
                        var objD = db.FlightDistance.Where(fd => fd.AirportID1 == TInfo.Flight.Route.Departure && fd.AirportID2 == TInfo.Flight.Route.Destination).FirstOrDefault();
                        if (objD == null)
                        {
                            objD = db.FlightDistance.Where(fd => fd.AirportID1 == TInfo.Flight.Route.Destination && fd.AirportID2 == TInfo.Flight.Route.Departure).FirstOrDefault();
                        }
                        totalDistance += objD.Distance;
                    }

                    // Subtract total distance from skymiles
                    var u = db.User.FirstOrDefault(user => user.UserID == o.UserID);
                    u.Skymiles = u.Skymiles - totalDistance;
                    db.SaveChanges();
                }


                // Add back AvailSeat to the Flights
                foreach (var item in TicketList)
                {
                    Flight FInfo = db.Flight.FirstOrDefault(f => f.FNo == item.FNo);
                    if (item.Class == "F")
                    {
                        FInfo.AvailSeatsF = FInfo.AvailSeatsF + 1;
                    }
                    else if (item.Class == "B")
                    {
                        FInfo.AvailSeatsB = FInfo.AvailSeatsB + 1;
                    }
                    else
                    {
                        FInfo.AvailSeatsE = FInfo.AvailSeatsE + 1;
                    }
                    db.SaveChanges();
                }

                // Refund to user account
                var firstFNo = TicketList.Where(t => t.IsReturn == false).FirstOrDefault().FNo;
                var flight = db.Flight.FirstOrDefault(f => f.FNo == firstFNo);
                var daysToDeparture = (flight.DepartureTime - DateTime.Now).TotalDays;
                if (daysToDeparture >= 14)
                {
                    refund = Convert.ToInt32(o.Total - o.Total * 0.02);
                }
                else
                {
                    refund = Convert.ToInt32(o.Total - o.Total * 0.02 * (14 - daysToDeparture));
                }
                var cardFinal = bank.BankDAO.GetCreditCard(db.User.FirstOrDefault(u => u.UserID == o.UserID).CCNo);
                cardFinal.Balance += refund;

                // Save changes
                db.SaveChanges();
                return "ok";
            }
            return "error";

        }
    }
}