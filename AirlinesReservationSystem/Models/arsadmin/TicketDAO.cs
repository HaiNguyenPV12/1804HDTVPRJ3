using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class TicketDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Ticket GetTicket(long ticketID) => db.Ticket.FirstOrDefault(s => s.TicketID == ticketID);

        public static IEnumerable<Ticket> GetTicketList(long orderID) => db.Ticket.Where(s => s.OrderID == orderID);


        public static bool UpdateTicket(Ticket updateT)
        {
            //var model = db.Ticket.Find(d);
            var s = GetTicket(updateT.TicketID);
            var d = db.Ticket.Where(item => item.OrderID == updateT.OrderID && item.Age < 14);
            if (s != null)
            {
                if(d != null)
                {                    
                    s.PassportNo = updateT.PassportNo;
                    foreach (var x in d)
                    {
                        x.PassportNo = updateT.PassportNo;
                    }
                }
                else
                {
                    s.PassportNo = updateT.PassportNo;
                }
                s.Firstname = updateT.Firstname;
                s.Lastname = updateT.Lastname;
                s.Sex = updateT.Sex;
                s.IsReturn = updateT.IsReturn;
                s.Age = updateT.Age;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}