using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.arsadmin
{
    public class InsertDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();
        public static List<ErrorModel> error = new List<ErrorModel>();
        static IEnumerable<FlightDistance> distanceDB = db.FlightDistance;
        static IEnumerable<Airline> airlineDB = db.Airline;
        static IEnumerable<Aircraft> aircraftDB = db.Aircraft;

        public static void InsertRoutes()
        {
            foreach (var distance in distanceDB)
            {
                var r = new Route();
                var r2 = new Route();
                var randgen = new Random();

                r.RAircraft = RandomAircraft(distance.Distance).AircraftID.Trim();
                r.RAirline = RandomAirline().AirlineID.Trim();

                r.Departure = distance.AirportID1.Trim();
                r.Destination = distance.AirportID2.Trim();

                RouteDAO.AddRoute(r);

                r2.RAircraft = RandomAircraft(distance.Distance).AircraftID.Trim();
                r2.RAirline = RandomAirline().AirlineID.Trim();

                r2.Departure = distance.AirportID2.Trim();
                r2.Destination = distance.AirportID1.Trim();
                RouteDAO.AddRoute(r2);
            }
        }

        public static void InsertFlight()
        {
            db = new AirlineDBEntities();
            IEnumerable<Route> routeDB = db.Route;

            foreach (var route in routeDB)
            {
                var aircraft = aircraftDB.Where(a => a.AircraftID == route.RAircraft).FirstOrDefault();
                int seatE = aircraft.EconomySeats;
                int seatB = 0, seatF = 0;
                var distance = FlightDistanceDAO.GetFlightDistance(route.Departure.Trim(), route.Destination.Trim());
                if (distance == null)
                {
                    string err = string.Format("{0}:{1}-{2}", route.RNo, route.Departure, route.Destination);
                    error.Add(new ErrorModel { Error = err });
                }
                else
                {
                    if (aircraft.BussinessSeats != null)
                    {
                        seatB = int.Parse(aircraft.BussinessSeats.ToString());
                    }
                    if (aircraft.FirstClassSeats != null)
                    {
                        seatF = int.Parse(aircraft.FirstClassSeats.ToString());
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        var dist = int.Parse(distance.Distance.ToString());
                        Flight f = new Flight();
                        f.FNo = RandomFNo(route.RAirline);
                        f.RNo = route.RNo;
                        f.AvailSeatsE = seatE;
                        f.AvailSeatsB = seatB;
                        f.AvailSeatsF = seatF;
                        f.BasePrice = RandomBasePrice(dist);
                        var departureTime = RandomDepartureTime();
                        f.DepartureTime = departureTime;
                        var arrivalTime = DateTime.Parse(departureTime.ToString());
                        f.ArrivalTime = arrivalTime.AddMinutes(MinutesToAdd(dist));
                        f.FlightTime = (f.ArrivalTime - f.DepartureTime).TotalHours;
                        FlightDAO.AddFlight(f);
                    }
                }
            }
        }

        static double MinutesToAdd(int distance)
        {
            double m,d=distance;
            
            m = d / 578 * 60;
            return m;
        }

        static double RandomBasePrice(int distance)
        {
            var randgen = new Random();
            double basePrice = distance * 0.07 + randgen.Next(-5, 5);
            return Math.Round(basePrice,2);
        }

        static string RandomFNo(string AirlineID)
        {
            string s;
            do
            {
                s = AirlineID;
                var randgen = new Random();
                int num = randgen.Next(0, 9999);
                if (num < 10)
                {
                    s += "000" + num;
                }
                else if (num < 100)
                {
                    s += "00" + num;
                }
                else if (num < 1000)
                {
                    s += "0" + num;
                }
                else
                {
                    s += num;
                }
            } while (FlightDAO.GetFlight(s) != null);
            return s;
        }

        static DateTime RandomDepartureTime()
        {
            var randgen = new Random();
            int year = 2019;
            int month = randgen.Next(8, 11);
            string monthStr = month < 10 ? "0" + month : month.ToString();
            var date = randgen.Next(1, DateTime.DaysInMonth(year, month) + 1);
            string dateStr = date < 10 ? "0" + date : date.ToString();
            int hour = randgen.Next(0, 24);
            string hourStr = hour < 10 ? "0" + hour : hour.ToString();
            int min;
            do
            {
                min = randgen.Next(0, 60);
            } while (min % 5 > 0);
            string minStr = min < 10 ? "0" + min : min.ToString();

            DateTime datetime = DateTime.Parse(string.Format("{0}/{1}/{2} {3}:{4}:00", monthStr, dateStr, year, hourStr, minStr));
            return datetime;
        }

        static Aircraft RandomAircraft(int distance)
        {
            var randgen = new Random();
            var aircraft = aircraftDB.ElementAt(randgen.Next(0, aircraftDB.Count()));
            while (aircraft.Range < distance)
            {
                aircraft = aircraftDB.ElementAt(randgen.Next(0, aircraftDB.Count()));
            }
            return aircraft;
        }

        static Airline RandomAirline()
        {
            var randgen = new Random();
            var airline = airlineDB.ElementAt(randgen.Next(0, airlineDB.Count()));
            return airline;
        }
    }
}