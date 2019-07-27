using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class RouteDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Route GetRoute(int RNo) => db.Route.FirstOrDefault(r => r.RNo == RNo);

        public static IEnumerable<Route> GetRouteList()
        {
            return db.Route;
        }

        public static IEnumerable<Route> GetRouteByFilter(string Airline)
        {
            return db.Route.Where(r => r.RAirline.ToLower() == Airline.Trim().ToLower());
        }

        public static bool DeleteRoute(int id)
        {
            var r = GetRoute(id);
            if (r != null)
            {
                if (db.Flight.FirstOrDefault(f => f.RNo == id) == null)
                {
                    db.Route.Remove(r);
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public static string AddRoute(Route newR)
        {
            try
            {
                string s = "";
                if (db.Airline.FirstOrDefault(al => al.AirlineID == newR.RAirline) == null)
                {
                    s += "Airline";
                }
                if (db.Aircraft.FirstOrDefault(ac => ac.AircraftID == newR.RAircraft) == null)
                {
                    if (s != "")
                        s += ", ";

                    s += "Aircraft";
                }
                //if (db.Airport.FirstOrDefault(ap1 => ap1.AirportID == newR.Departure) == null)
                //{
                //    if (s != "")
                //        s += ", ";

                //    s += "Departure";
                //}
                //if (db.Airport.FirstOrDefault(ap2 => ap2.AirportID == newR.Destination) == null)
                //{
                //    if (s != "")
                //        s += ", ";

                //    s += "Destination";
                //}
                if (db.FlightDistance.Where(fd => fd.AirportID1 == newR.Departure && fd.AirportID2 == newR.Destination).FirstOrDefault() == null)
                {
                    if (db.FlightDistance.Where(fd => fd.AirportID2 == newR.Departure && fd.AirportID1 == newR.Destination).FirstOrDefault() == null)
                    {
                        if (s != "")
                            s += ", ";
                        s += "Departure and Destination";
                    }
                }


                if (s != "")
                {
                    throw new Exception("Please choose valid " + s + ".");
                }

                db.Route.Add(newR);
                db.SaveChanges();
                return "ok";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string UpdateRoute(Route updateR)
        {
            try
            {
                var r = GetRoute(updateR.RNo);
                if (r == null)
                {
                    throw new Exception("Cannot find this route's ID");
                }
                string s = "";
                if (db.Airline.FirstOrDefault(al => al.AirlineID == updateR.RAirline) == null)
                {
                    s += "Airline";
                }
                if (db.Aircraft.FirstOrDefault(ac => ac.AircraftID == updateR.RAircraft) == null)
                {
                    if (s != "")
                        s += ", ";

                    s += "Aircraft";
                }
                //if (db.Airport.FirstOrDefault(ap1 => ap1.AirportID == updateR.Departure) == null)
                //{
                //    if (s != "")
                //        s += ", ";

                //    s += "Departure";
                //}
                //if (db.Airport.FirstOrDefault(ap2 => ap2.AirportID == updateR.Destination) == null)
                //{
                //    if (s != "")
                //        s += ", ";

                //    s += "Destination";
                //}

                if (db.FlightDistance.Where(fd => fd.AirportID1 == updateR.Departure && fd.AirportID2 == updateR.Destination).FirstOrDefault() == null)
                {
                    if (db.FlightDistance.Where(fd => fd.AirportID2 == updateR.Departure && fd.AirportID1 == updateR.Destination).FirstOrDefault() == null)
                    {
                        if (s != "")
                            s += ", ";
                        s += "Departure and Destination";
                    }
                }

                if (s != "")
                {
                    throw new Exception("Please choose valid " + s + ".");
                }

                r.RAirline = updateR.RAirline;
                r.RAircraft = updateR.RAircraft;
                r.Departure = updateR.Departure;
                r.Destination = updateR.Destination;
                db.SaveChanges();
                return "ok";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}