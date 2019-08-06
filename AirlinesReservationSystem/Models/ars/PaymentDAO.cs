using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirlinesReservationSystem.Models.arsadmin;

namespace AirlinesReservationSystem.Models.ars
{
    public class PaymentDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Order GetOrder(Int64 id) {
            db = new AirlineDBEntities();
            return db.Order.FirstOrDefault(o => o.OrderID == id);
        }

        public static IEnumerable<Ticket> GetTicketList(Int64 id) => db.Ticket.Where(t => t.OrderID == id);

        public static double GetFlightServiceFee(int RNo) => db.Route.FirstOrDefault(r => r.RNo == RNo).Aircraft.ServiceFee;

        public static Airport GetFlightDeparture(int RNo)
        {
            string id = db.Route.FirstOrDefault(r => r.RNo == RNo).Departure;
            return db.Airport.FirstOrDefault(a => a.AirportID == id);
        }
        public static Airport GetFlightDestination(int RNo)
        {
            string id = db.Route.FirstOrDefault(r => r.RNo == RNo).Destination;
            return db.Airport.FirstOrDefault(a => a.AirportID == id);
        }

        public static string BlockingOrderPaid(long id, string CCNo, string CVV)
        {
            db = new AirlineDBEntities();
            var o = GetOrder(id);
            CreditCard card = db.CreditCard.FirstOrDefault(c => c.CCNo == CCNo);
            if (card == null || card.CVV != CVV)
            {
                return "Error: Credit Card is not valid.";
            }
            if (card.Balance < o.Total)
            {
                return "Error: Not enough money in account.";
            }
            if (o != null)
            {
                // Change status
                o.Status = 1;
                int totalDistance = 0;

                // Calculate flight distance (flightdistance x total people)
                var TicketList = TicketDAO.GetTicketList(o.OrderID);
                //int totalCount = TicketList.Count();
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

                // Add total distance to skymiles in User
                db.User.FirstOrDefault(u => u.UserID == o.UserID).Skymiles += totalDistance;

                // Charging
                card.Balance = card.Balance - o.Total;

                db.SaveChanges();
                return "ok";
            }
            return "Error: Order ID not valid.";
        }

        public static string CancelOrder(long id, string CCNo)
        {
            db = new AirlineDBEntities();
            var o = GetOrder(id);
            CreditCard card = db.CreditCard.FirstOrDefault(c => c.CCNo == CCNo);
            if (o.Status == 1)
            {
                if (card == null)
                {
                    return "Error: Credit Card is not valid.";
                }
            }
            if (o != null)
            {
                // Check if this order is blocked or not
                bool isBlocked = false;
                if (o.Status == 0)
                {
                    isBlocked = true;
                }
                // Change status
                o.Status = 2;
                int totalDistance = 0;
                double refund = 0;
                var TicketList = TicketDAO.GetTicketList(o.OrderID);

                // If this order is not blocked one, make changes to skymiles
                if (!isBlocked)
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

                // Refund to user account if this is paid order
                if (!isBlocked)
                {
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
                    var cardFinal = db.CreditCard.FirstOrDefault(c => c.CCNo == CCNo);
                    cardFinal.Balance += refund;
                }

                // Save changes
                db.SaveChanges();
                return "ok";
            }
            return "error";
        }

        // Function that process Payment request
        // Will return "ok:[orderid]" if success
        // Will return any other error string when failed
        public static string ProcessPayment(Payment payment, bool isBlocked)
        {
            db = new AirlineDBEntities();
            // Checking Creditcard
            CreditCard card = db.CreditCard.FirstOrDefault(c => c.CCNo == payment.CCNo);
            if (!isBlocked)
            {
                if (card == null || card.CVV != payment.CVV)
                {
                    return "Error: Credit Card is not valid.";
                }
                if (card.Balance < payment.Total)
                {
                    return "Error: Not enough money in account.";
                }
            }

            string s = "";
            try
            {
                // Create Order
                DateTime now = DateTime.Now;
                string OrderIDStr = "" + now.Year + now.Month + now.Day + now.Hour + now.Minute + now.Second;
                Int64 OrderID = Int64.Parse(OrderIDStr);
                double Total = 0;

                Order order = new Order();
                order.OrderID = OrderID;
                order.OrderDate = now;
                if (isBlocked)
                {
                    order.Status = 0;
                }
                else
                {
                    order.Status = 1;
                }

                order.UserID = payment.UserID;
                order.Total = Total;

                db.Order.Add(order);
                int ticketCount = 1;

                // Create Oneway / First Flight/ First Stop Ticket
                Flight FInfo1 = FlightDAO.GetFlight(payment.FNo1);
                payment.Passengers.ForEach(p =>
                {
                    Ticket ticket = new Ticket();
                    Int64 TicketID = Int64.Parse(OrderID.ToString() + ticketCount);
                    double price = 0;
                    if (p.Age >= 14)
                    {
                        price += FInfo1.BasePrice + FInfo1.Route.Aircraft.ServiceFee;
                    }
                    else
                    {
                        price += FInfo1.BasePrice * 70 / 100 + FInfo1.Route.Aircraft.ServiceFee;
                    }
                    double daysToDeparture = (FInfo1.DepartureTime - DateTime.Now).TotalDays;
                    if (daysToDeparture <= 14)
                    {
                        if (p.Age >= 14)
                        {
                            price += FInfo1.BasePrice * 0.02 * (14 - daysToDeparture);
                        }
                        else
                        {
                            price += FInfo1.BasePrice * 70 / 100 * 0.02 * (14 - daysToDeparture);
                        }
                    }

                    foreach (var item in p.Service)
                    {
                        price += ServiceDAO.GetService(item).ServiceFee;
                        db.TicketService.Add(new TicketService { ServiceID = item, TicketID = TicketID });
                    }
                    if (payment.Class == "F")
                    {
                        price += 20;
                    }
                    else if (payment.Class == "B")
                    {
                        price += 10;
                    }
                    ticket.TicketID = TicketID;
                    ticket.OrderID = OrderID;
                    ticket.FNo = FInfo1.FNo;
                    ticket.PassportNo = p.PassportNo;
                    ticket.Class = payment.Class;
                    ticket.Firstname = p.Firstname;
                    ticket.Lastname = p.Lastname;
                    ticket.Sex = p.Sex;
                    ticket.Age = p.Age;
                    ticket.IsReturn = false;
                    ticket.Price = price;
                    db.Ticket.Add(ticket);
                    Total += price;
                    ticketCount++;
                });

                // Create Second stop's ticket (if exists)
                Flight FInfo2 = FlightDAO.GetFlight(payment.FNo2);
                if (FInfo2 != null)
                {
                    payment.Passengers.ForEach(p =>
                    {
                        Ticket ticket = new Ticket();
                        Int64 TicketID = Int64.Parse(OrderID.ToString() + ticketCount);
                        double price = 0;
                        if (p.Age >= 14)
                        {
                            price += FInfo2.BasePrice + FInfo2.Route.Aircraft.ServiceFee;
                        }
                        else
                        {
                            price += FInfo2.BasePrice * 70 / 100 + FInfo2.Route.Aircraft.ServiceFee;
                        }
                        double daysToDeparture = (FInfo2.DepartureTime - DateTime.Now).TotalDays;
                        if (daysToDeparture <= 14)
                        {
                            if (p.Age >= 14)
                            {
                                price += FInfo2.BasePrice * 0.02 * (14 - daysToDeparture);
                            }
                            else
                            {
                                price += FInfo2.BasePrice * 70 / 100 * 0.02 * (14 - daysToDeparture);
                            }
                        }

                        foreach (var item in p.Service)
                        {
                            price += ServiceDAO.GetService(item).ServiceFee;
                            db.TicketService.Add(new TicketService { ServiceID = item, TicketID = TicketID });
                        }
                        if (payment.Class == "F")
                        {
                            price += 20;
                        }
                        else if (payment.Class == "B")
                        {
                            price += 10;
                        }
                        ticket.TicketID = TicketID;
                        ticket.OrderID = OrderID;
                        ticket.FNo = FInfo2.FNo;
                        ticket.PassportNo = p.PassportNo;
                        ticket.Class = payment.Class;
                        ticket.Firstname = p.Firstname;
                        ticket.Lastname = p.Lastname;
                        ticket.Sex = p.Sex;
                        ticket.Age = p.Age;
                        ticket.IsReturn = false;
                        ticket.Price = price;
                        db.Ticket.Add(ticket);
                        Total += price;
                        ticketCount++;
                    });
                }

                // Create Return Flight Ticket
                Flight ReFInfo = FlightDAO.GetFlight(payment.ReFNo);
                if (ReFInfo != null)
                {
                    payment.Passengers.ForEach(p =>
                    {
                        Ticket ticket = new Ticket();
                        Int64 TicketID = Int64.Parse(OrderID.ToString() + ticketCount);
                        double price = 0;
                        if (p.Age >= 14)
                        {
                            price += ReFInfo.BasePrice + ReFInfo.Route.Aircraft.ServiceFee;
                        }
                        else
                        {
                            price += ReFInfo.BasePrice * 70 / 100 + ReFInfo.Route.Aircraft.ServiceFee;
                        }

                        double daysToDeparture = (ReFInfo.DepartureTime - DateTime.Now).TotalDays;
                        if (daysToDeparture <= 14)
                        {
                            if (p.Age >= 14)
                            {
                                price += ReFInfo.BasePrice * 0.02 * (14 - daysToDeparture);
                            }
                            else
                            {
                                price += ReFInfo.BasePrice * 70 / 100 * 0.02 * (14 - daysToDeparture);
                            }
                        }

                        foreach (var item in p.Service)
                        {
                            price += ServiceDAO.GetService(item).ServiceFee;
                            db.TicketService.Add(new TicketService { ServiceID = item, TicketID = TicketID });
                        }
                        if (payment.Class == "F")
                        {
                            price += 20;
                        }
                        else if (payment.Class == "B")
                        {
                            price += 10;
                        }
                        ticket.TicketID = TicketID;
                        ticket.OrderID = OrderID;
                        ticket.FNo = ReFInfo.FNo;
                        ticket.PassportNo = p.PassportNo;
                        ticket.Class = payment.Class;
                        ticket.Firstname = p.Firstname;
                        ticket.Lastname = p.Lastname;
                        ticket.Sex = p.Sex;
                        ticket.Age = p.Age;
                        ticket.IsReturn = true;
                        ticket.Price = price;
                        db.Ticket.Add(ticket);
                        Total += price;
                        ticketCount++;
                    });
                }
                db.SaveChanges();

                db.Order.FirstOrDefault(o => o.OrderID == OrderID).Total = Total;

                // Seat Calculation
                var SFInfo1 = db.Flight.Where(f => f.FNo == payment.FNo1).FirstOrDefault();
                var SFInfo2 = db.Flight.Where(f => f.FNo == payment.FNo2).FirstOrDefault();
                var SReFInfo = db.Flight.Where(f => f.FNo == payment.ReFNo).FirstOrDefault();
                if (payment.Class == "F")
                {
                    SFInfo1.AvailSeatsF = SFInfo1.AvailSeatsF - payment.Passengers.Count();

                    if (SFInfo2 != null)
                    {
                        SFInfo2.AvailSeatsF = SFInfo2.AvailSeatsF - payment.Passengers.Count();
                    }
                    if (SReFInfo != null)
                    {
                        SReFInfo.AvailSeatsF = SReFInfo.AvailSeatsF - payment.Passengers.Count();
                    }
                }
                else if (payment.Class == "B")
                {
                    SFInfo1.AvailSeatsB = SFInfo1.AvailSeatsB - payment.Passengers.Count();
                    if (SFInfo2 != null)
                    {
                        SFInfo2.AvailSeatsB = SFInfo2.AvailSeatsB - payment.Passengers.Count();
                    }
                    if (SReFInfo != null)
                    {
                        SReFInfo.AvailSeatsB = SReFInfo.AvailSeatsB - payment.Passengers.Count();
                    }
                }
                else
                {
                    SFInfo1.AvailSeatsE = SFInfo1.AvailSeatsE - payment.Passengers.Count();
                    if (SFInfo2 != null)
                    {
                        SFInfo2.AvailSeatsE = SFInfo2.AvailSeatsE - payment.Passengers.Count();
                    }
                    if (SReFInfo != null)
                    {
                        SReFInfo.AvailSeatsE = SReFInfo.AvailSeatsE - payment.Passengers.Count();
                    }
                }
                db.SaveChanges();


                // Skymile
                if (order.Status == 1)
                {
                    int dis1 = 0;
                    int dis2 = 0;
                    int reDis = 0;
                    var objD1 = db.FlightDistance.Where(fd => fd.AirportID1 == FInfo1.Route.Departure && fd.AirportID2 == FInfo1.Route.Destination).FirstOrDefault();
                    if (objD1 == null)
                    {
                        objD1 = db.FlightDistance.Where(fd => fd.AirportID1 == FInfo1.Route.Destination && fd.AirportID2 == FInfo1.Route.Departure).FirstOrDefault();
                    }
                    dis1 = objD1.Distance * payment.Passengers.Count();

                    if (FInfo2 != null)
                    {
                        var objD2 = db.FlightDistance.Where(fd => fd.AirportID1 == FInfo2.Route.Departure && fd.AirportID2 == FInfo2.Route.Destination).FirstOrDefault();
                        if (objD2 == null)
                        {
                            objD2 = db.FlightDistance.Where(fd => fd.AirportID1 == FInfo2.Route.Destination && fd.AirportID2 == FInfo2.Route.Departure).FirstOrDefault();
                        }
                        dis2 = objD2.Distance * payment.Passengers.Count();
                    }

                    if (ReFInfo != null)
                    {
                        var objReD = db.FlightDistance.Where(fd => fd.AirportID1 == ReFInfo.Route.Departure && fd.AirportID2 == ReFInfo.Route.Destination).FirstOrDefault();
                        if (objReD == null)
                        {
                            objReD = db.FlightDistance.Where(fd => fd.AirportID1 == ReFInfo.Route.Destination && fd.AirportID2 == ReFInfo.Route.Departure).FirstOrDefault();
                        }
                        reDis = objReD.Distance * payment.Passengers.Count();
                    }

                    db.User.FirstOrDefault(u => u.UserID == order.UserID).Skymiles += dis1 + dis2 + reDis;
                }
                db.SaveChanges();

                // Charging creditcard
                if (!isBlocked)
                {
                    card.Balance = card.Balance - Total;
                }

                // Saving changes
                db.SaveChanges();
                return "ok:" + OrderID;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        //public static string ConfirmPayment(Payment payment)
        //{
        //    try
        //    {
        //        string s = "";
        //        DateTime now = DateTime.Now;
        //        string OrderIDStr = "" + now.Year + now.Month + now.Day + now.Hour + now.Minute + now.Second;
        //        Int64 OrderID = Int64.Parse(OrderIDStr);
        //        Flight FInfo = FlightDAO.GetFlight(payment.FNo1);
        //        double FTotal = 0;
        //        double ReFTotal = 0;

        //        s += "+ 1. ORDER: " + OrderID + "\n";
        //        s += "+ Status: Booked\n";
        //        s += "+ Order Date: " + now.ToString() + "\n";
        //        s += "+ + + + + + + + + + +\n\n";

        //        s += "+ 2.TICKETS:\n";
        //        int ticketCount = 1;
        //        payment.Passengers.ForEach(p =>
        //        {
        //            double Price = FInfo.BasePrice;
        //            double MaintainFee = FInfo.Route.Aircraft.ServiceFee;
        //            s += "++ + + + + + + + + + +\n";
        //            s += "++ Ticket " + ticketCount + "\n";
        //            s += "++ Airline: " + FInfo.Route.Airline.AirlineName + "(" + FInfo.Route.Aircraft.AircraftName + ")\n";
        //            s += "++ Flight: " + FInfo.Route.DepartureAirport.AirportID + "(" + FInfo.DepartureTime + ") => " + FInfo.Route.DestinationAirport.AirportID + "(" + FInfo.ArrivalTime + ")\n";
        //            s += "++ Passenger: " + p.Firstname + " " + p.Lastname + "\n";
        //            s += "++ Sex: " + p.Sex + ", Age: " + p.Age + "Passport No.: " + p.PassportNo + "\n";
        //            s += "++ Service: ";
        //            double ServiceFee = 0;
        //            foreach (var item in p.Service)
        //            {
        //                var service = ServiceDAO.GetService(item);
        //                s += item + ": " + service.ServiceName;
        //                ServiceFee += service.ServiceFee;
        //                if (item != p.Service.Last())
        //                {
        //                    s += ", ";
        //                }
        //            }
        //            Price += ServiceFee;
        //            s += "\n";

        //            s += "++ Airport maintainance fee: " + MaintainFee + "\n";
        //            Price += MaintainFee;

        //            if (payment.Class == "F")
        //            {
        //                s += "++ First Class's Fee: 20\n";
        //                Price += 20;
        //            }
        //            else if (payment.Class == "B")
        //            {
        //                s += "++ Bussiness Class's Fee: 10\n";
        //                Price += 10;
        //            }
        //            else if (payment.Class == "E")
        //            {
        //                s += "++ Economy Class's Fee: 0\n";
        //            }

        //            s += "++ Ticket Price: " + Price + "\n";
        //            s += "++ + + + + + + + + + +\n\n";
        //            FTotal += Price;
        //            ticketCount++;
        //        });

        //        if (!string.IsNullOrEmpty(payment.ReFNo))
        //        {
        //            Flight ReFInfo = FlightDAO.GetFlight(payment.ReFNo);
        //            s += "+ 3.RETURN FLIGHT's TICKETS:\n";
        //            payment.Passengers.ForEach(p =>
        //            {
        //                double Price = ReFInfo.BasePrice;
        //                double MaintainFee = FInfo.Route.Aircraft.ServiceFee;
        //                s += "++ + + + + + + + + + +\n";
        //                s += "++ Ticket " + ticketCount + "\n";
        //                s += "++ Airline: " + ReFInfo.Route.Airline.AirlineName + "(" + ReFInfo.Route.Aircraft.AircraftName + ")\n";
        //                s += "++ Flight: " + ReFInfo.Route.DepartureAirport.AirportID + "(" + ReFInfo.DepartureTime + ") => " + ReFInfo.Route.DestinationAirport.AirportID + "(" + ReFInfo.ArrivalTime + ")\n";
        //                s += "++ Passenger: " + p.Firstname + " " + p.Lastname + "\n";
        //                s += "++ Sex: " + p.Sex + ", Age: " + p.Age + "Passport No.: " + p.PassportNo + "\n";
        //                s += "++ Service: ";
        //                foreach (var item in p.Service)
        //                {
        //                    var service = ServiceDAO.GetService(item);
        //                    s += service.ServiceName;
        //                    Price += service.ServiceFee;
        //                    if (item != p.Service.Last())
        //                    {
        //                        s += ", ";
        //                    }
        //                }
        //                s += "\n";
        //                s += "++ Airport maintainance fee: " + MaintainFee + "\n";
        //                Price += MaintainFee;

        //                if (payment.Class == "F")
        //                {
        //                    s += "++ First Class's Fee: 20\n";
        //                    Price += 20;
        //                }
        //                else if (payment.Class == "B")
        //                {
        //                    s += "++ Bussiness Class's Fee: 10\n";
        //                    Price += 10;
        //                }
        //                else if (payment.Class == "E")
        //                {
        //                    s += "++ Economy Class's Fee: 0\n";
        //                }

        //                s += "++ Ticket Price: " + Price + "\n";
        //                s += "++ + + + + + + + + + +\n\n";
        //                ReFTotal += Price;
        //                ticketCount++;
        //            });
        //        }
        //        s += "TOTAL: " + (FTotal + ReFTotal);
        //        //Order order = new Order();
        //        //order.OrderID = OrderID;

        //        return s;
        //    }
        //    catch (Exception e)
        //    {
        //        return e.StackTrace;
        //    }
        //}
    }
}