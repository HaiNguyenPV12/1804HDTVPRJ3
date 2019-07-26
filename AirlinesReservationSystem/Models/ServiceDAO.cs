using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models
{
    public class ServiceDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Service GetService(string ServiceID) => db.Service.FirstOrDefault(s => s.ServiceID == ServiceID);

        public static IEnumerable<Service> GetServiceList() => db.Service.OrderByDescending(s => s.IsServing);

        public static bool DeleteService(string id)
        {
            var s = GetService(id);
            if (s != null)
            {
                db.Service.Remove(s);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public static bool AddService(Service newS)
        {
            if (GetService(newS.ServiceID) == null)
            {
                db.Service.Add(newS);
                db.SaveChanges();
                return true;
            }
            return false;
        }
        public static string GetNextServiceID()
        {
            string nextID = "";
            if (db.Service.Count() > 0)
            {
                string lastID = db.Service.OrderByDescending(s=>s.ServiceID).FirstOrDefault().ServiceID;
                string sufID = lastID.Substring(1, 2);
                int nextNum = int.Parse(sufID) + 1;
                if (nextNum < 10)
                {
                    nextID = "S0" + nextNum;
                }
                else
                {
                    nextID = "S" + nextNum;
                }
            }
            else
            {
                nextID = "S00";
            }
            
            return nextID;
        }
    }
}