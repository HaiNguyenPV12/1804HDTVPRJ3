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
            var s = GetTicket(updateT.TicketID);
            if (s != null)
            {
                s.PassportNo = updateT.PassportNo;
                s.Class = updateT.Class;
                s.Firstname = updateT.Firstname;
                s.Lastname = updateT.Lastname;
                s.Sex = updateT.Sex;
                s.Age = updateT.Age;
                s.IsReturn = updateT.IsReturn;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}