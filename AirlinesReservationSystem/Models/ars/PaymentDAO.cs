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

        public static Order GetOrder(Int64 id) => db.Order.FirstOrDefault(o => o.OrderID == id);

        public static IEnumerable<Ticket> GetTicketList(Int64 id) => db.Ticket.Where(t => t.OrderID == id);


        public static string ProcessPayment(Payment payment, bool IsBlocked)
        {
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
                if (IsBlocked)
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

                // Create First Flight Ticket
                Flight FInfo = FlightDAO.GetFlight(payment.FNo);
                payment.Passengers.ForEach(p =>
                {
                    Ticket ticket = new Ticket();
                    Int64 TicketID = Int64.Parse(OrderID.ToString() + ticketCount);
                    double price = 0;
                    price += FInfo.BasePrice + FInfo.Route.Aircraft.ServiceFee;
                    foreach (var item in p.Service)
                    {
                        price += ServiceDAO.GetService(item).ServiceFee;
                        db.TicketService.Add(new TicketService { ServiceID = item, TicketID = TicketID });
                    }
                    if (p.Class == "F")
                    {
                        price += 20;
                    }
                    else if (p.Class == "B")
                    {
                        price += 10;
                    }
                    ticket.TicketID = TicketID;
                    ticket.OrderID = OrderID;
                    ticket.FNo = FInfo.FNo;
                    ticket.PassportNo = p.PassportNo;
                    ticket.Class = p.Class;
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

                // Create Return Flight Ticket
                Flight ReFInfo = FlightDAO.GetFlight(payment.ReFNo);
                if (ReFInfo != null)
                {
                    payment.Passengers.ForEach(p =>
                    {
                        Ticket ticket = new Ticket();
                        Int64 TicketID = Int64.Parse(OrderID.ToString() + ticketCount);
                        double price = 0;
                        price += ReFInfo.BasePrice + ReFInfo.Route.Aircraft.ServiceFee;
                        foreach (var item in p.Service)
                        {
                            price += ServiceDAO.GetService(item).ServiceFee;
                            db.TicketService.Add(new TicketService { ServiceID = item, TicketID = TicketID });
                        }
                        if (p.ReClass == "F")
                        {
                            price += 20;
                        }
                        else if (p.ReClass == "B")
                        {
                            price += 10;
                        }
                        ticket.TicketID = TicketID;
                        ticket.OrderID = OrderID;
                        ticket.FNo = ReFInfo.FNo;
                        ticket.PassportNo = p.PassportNo;
                        ticket.Class = p.ReClass;
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
                db.SaveChanges();

                return "ok:" + OrderID;
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
        }

        public static string ConfirmPayment(Payment payment)
        {
            try
            {
                string s = "";
                DateTime now = DateTime.Now;
                string OrderIDStr = "" + now.Year + now.Month + now.Day + now.Hour + now.Minute + now.Second;
                Int64 OrderID = Int64.Parse(OrderIDStr);
                Flight FInfo = FlightDAO.GetFlight(payment.FNo);
                double FTotal = 0;
                double ReFTotal = 0;

                s += "+ 1. ORDER: " + OrderID + "\n";
                s += "+ Status: Booked\n";
                s += "+ Order Date: " + now.ToString() + "\n";
                s += "+ + + + + + + + + + +\n\n";

                s += "+ 2.TICKETS:\n";
                int ticketCount = 1;
                payment.Passengers.ForEach(p =>
                {
                    double Price = FInfo.BasePrice;
                    double MaintainFee = FInfo.Route.Aircraft.ServiceFee;
                    s += "++ + + + + + + + + + +\n";
                    s += "++ Ticket " + ticketCount + "\n";
                    s += "++ Airline: " + FInfo.Route.Airline.AirlineName + "(" + FInfo.Route.Aircraft.AircraftName + ")\n";
                    s += "++ Flight: " + FInfo.Route.DepartureAirport.AirportID + "(" + FInfo.DepartureTime + ") => " + FInfo.Route.DestinationAirport.AirportID + "(" + FInfo.ArrivalTime + ")\n";
                    s += "++ Passenger: " + p.Firstname + " " + p.Lastname + "\n";
                    s += "++ Sex: " + p.Sex + ", Age: " + p.Age + "Passport No.: " + p.PassportNo + "\n";
                    s += "++ Service: ";
                    double ServiceFee = 0;
                    foreach (var item in p.Service)
                    {
                        var service = ServiceDAO.GetService(item);
                        s += item + ": " + service.ServiceName;
                        ServiceFee += service.ServiceFee;
                        if (item != p.Service.Last())
                        {
                            s += ", ";
                        }
                    }
                    Price += ServiceFee;
                    s += "\n";

                    s += "++ Airport maintainance fee: " + MaintainFee + "\n";
                    Price += MaintainFee;

                    if (p.Class == "F")
                    {
                        s += "++ First Class's Fee: 20\n";
                        Price += 20;
                    }
                    else if (p.Class == "B")
                    {
                        s += "++ Bussiness Class's Fee: 10\n";
                        Price += 10;
                    }
                    else if (p.Class == "E")
                    {
                        s += "++ Economy Class's Fee: 0\n";
                    }

                    s += "++ Ticket Price: " + Price + "\n";
                    s += "++ + + + + + + + + + +\n\n";
                    FTotal += Price;
                    ticketCount++;
                });

                if (!string.IsNullOrEmpty(payment.ReFNo))
                {
                    Flight ReFInfo = FlightDAO.GetFlight(payment.ReFNo);
                    s += "+ 3.RETURN FLIGHT's TICKETS:\n";
                    payment.Passengers.ForEach(p =>
                    {
                        double Price = ReFInfo.BasePrice;
                        double MaintainFee = FInfo.Route.Aircraft.ServiceFee;
                        s += "++ + + + + + + + + + +\n";
                        s += "++ Ticket " + ticketCount + "\n";
                        s += "++ Airline: " + ReFInfo.Route.Airline.AirlineName + "(" + ReFInfo.Route.Aircraft.AircraftName + ")\n";
                        s += "++ Flight: " + ReFInfo.Route.DepartureAirport.AirportID + "(" + ReFInfo.DepartureTime + ") => " + ReFInfo.Route.DestinationAirport.AirportID + "(" + ReFInfo.ArrivalTime + ")\n";
                        s += "++ Passenger: " + p.Firstname + " " + p.Lastname + "\n";
                        s += "++ Sex: " + p.Sex + ", Age: " + p.Age + "Passport No.: " + p.PassportNo + "\n";
                        s += "++ Service: ";
                        foreach (var item in p.Service)
                        {
                            var service = ServiceDAO.GetService(item);
                            s += service.ServiceName;
                            Price += service.ServiceFee;
                            if (item != p.Service.Last())
                            {
                                s += ", ";
                            }
                        }
                        s += "\n";
                        s += "++ Airport maintainance fee: " + MaintainFee + "\n";
                        Price += MaintainFee;

                        if (p.ReClass == "F")
                        {
                            s += "++ First Class's Fee: 20\n";
                            Price += 20;
                        }
                        else if (p.ReClass == "B")
                        {
                            s += "++ Bussiness Class's Fee: 10\n";
                            Price += 10;
                        }
                        else if (p.ReClass == "E")
                        {
                            s += "++ Economy Class's Fee: 0\n";
                        }

                        s += "++ Ticket Price: " + Price + "\n";
                        s += "++ + + + + + + + + + +\n\n";
                        ReFTotal += Price;
                        ticketCount++;
                    });
                }
                s += "TOTAL: " + (FTotal + ReFTotal);
                //Order order = new Order();
                //order.OrderID = OrderID;

                return s;
            }
            catch (Exception e)
            {
                return e.StackTrace;
            }
        }

        //public static string ConfirmPayment(FormCollection frmPayment)
        //{
        //    try
        //    {
        //        string s = "";
        //        int PeopleNum = int.Parse(frmPayment["PeopleNum"]);
        //        string[] Firstname = frmPayment["Firstname"].Split(',');
        //        string[] Lastname = frmPayment["Lastname"].Split(',');
        //        string[] Sex = frmPayment["Sex"].Split(',');
        //        string[] Age = frmPayment["Age"].Split(',');
        //        string[][] Service = new string[PeopleNum][];
        //        for (int i = 0; i < PeopleNum; i++)
        //        {
        //            Service[i] = frmPayment["Service" + (i + 1)].Split(',');
        //        }
        //        for (int i = 0; i < PeopleNum; i++)
        //        {
        //            s += "Passenger " + (i + 1) + "\n";
        //            s += "Name: " + Firstname[i] + " " + Lastname[i] + "\n";
        //            s += "Sex: " + Sex[i] + ", Age: " + Age[i] + "\n";
        //            s += "Service: ";
        //            foreach (var item in Service[i])
        //            {
        //                if (item == Service[i].Last())
        //                {
        //                    s += item + "\n";
        //                }
        //                else
        //                {
        //                    s += item + ", ";
        //                }
        //            }
        //        }
        //        return s;
        //    }
        //    catch (Exception e)
        //    {
        //        return e.Message;
        //    }

        //}
    }
}