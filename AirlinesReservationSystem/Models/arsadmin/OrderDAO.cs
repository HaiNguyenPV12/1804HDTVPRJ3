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

        public static bool CancelledOrder(long cancelOrder)
        {
            var s = GetOrder(cancelOrder);
            if (s != null)
            {
                s.Status = 2;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}