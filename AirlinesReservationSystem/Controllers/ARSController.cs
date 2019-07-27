using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.ars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Controllers
{
    public class ARSController : Controller
    {
        // GET: Home
        public ActionResult Index() => View();

        [HttpPost]
        public ActionResult Index(FlightSearch flightSearch)
        {
            Session["searchParams"] = flightSearch;
            int totalPassenger = flightSearch.Adult + flightSearch.Children + flightSearch.Senior;
            if (ModelState.IsValid && totalPassenger > 0)
            {
                Session["searchParams"] = flightSearch;
                return RedirectToAction("FlightList", flightSearch);
            }
            else
            {
                if (totalPassenger <= 0)
                    ModelState.AddModelError("", "Passengers numbers are invalid");
                ModelState.AddModelError("", "Please enter required values");
            }
            return View(flightSearch);
        }

        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(User login)
        {
            ModelState.Remove("Firstname");
            ModelState.Remove("Lastname");
            ModelState.Remove("Address");
            ModelState.Remove("Phone");
            ModelState.Remove("Email");
            ModelState.Remove("Sex");
            ModelState.Remove("Age");
            ModelState.Remove("CCNo");
            ModelState.Remove("PassportNo_");
            ModelState.Remove("Skymiles");
            if (ModelState.IsValid)
            {
                var user = UsersDAO.CheckLogin(login.UserID, login.Password); //TODO get user from database and check password 
                if (user != null)
                {
                    Session["user"] = user.UserID;
                    return RedirectToAction("Index"); //TODO redirect to previous page instead of home
                }
                ModelState.AddModelError("", "Invalid login information");
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            return RedirectToAction("Index");
        }


        public ActionResult FlightList(FlightSearch flightSearch)
        {
            //TODO sub string to get Airport IDs
            ViewBag.RoundTrip = flightSearch.IsRoundTrip;
            var model = FlightSearchDAO.GetFlightResults(flightSearch);
            Session["searchResultsFirstTrip"] = model;
            return View(model);
        }

        public ActionResult FlightListReturn(string fid, int rid)
        {
            try
            {
                FlightResult firstTrip = FlightSearchDAO.GetFlightResult(fid, rid);
                FlightSearch flightSearch = (FlightSearch)Session["searchParams"];
                var seatsLeft = firstTrip.FlightVM.AvailSeatsB + firstTrip.FlightVM.AvailSeatsF + firstTrip.FlightVM.AvailSeatsE;
                if (seatsLeft == 0)
                {
                    TempData["NoSeatsMessage"] = "Sorry, there are no more seats left for flight " + firstTrip.FlightVM.FNo;
                    return RedirectToAction("FlightList", flightSearch);
                }
                ViewBag.firstTrip = firstTrip;
                var roundTripEnd = flightSearch.Departure;
                var roundTripStart = flightSearch.Destination;
                flightSearch.Departure = roundTripStart;
                flightSearch.Destination = roundTripEnd;
                flightSearch.DepartureTime = flightSearch.ReturnDepartureTime;
                var model = FlightSearchDAO.GetFlightResults(flightSearch);
                return View(model);
            }
            catch (Exception)
            {
                TempData["errorM"] = "There was an error executing your requests. Please try again";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult FlightList(FlightSearchDetails flightSearchDetails)
        {
            return View();
        }

        public ActionResult FlightDetails(string fid, int rid)
        {
            var model = FlightSearchDAO.GetFlightResult(fid, rid);
            return View(model);
        }

        public ActionResult TypeAheadDemo() => View();
        public ActionResult JqueryUIDemo() => View();

        public ActionResult Register() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User newU)
        {
            if (ModelState.IsValid)
            {
                if (UsersDAO.AddUser(newU))
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Register Error");
            }
            return View();
        }

        public ActionResult ProfileUser()
        {
            if (Session["user"] != null)
            {
                var model = UsersDAO.GetUser(Session["user"].ToString());
                if (model != null)
                {
                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult EditUser()
        {
            if (Session["user"] != null)
            {
                var model = UsersDAO.GetUser(Session["user"].ToString());
                if (model != null)
                {
                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User updateU)
        {
            if (ModelState.IsValid)
            {
                if (UsersDAO.UpdateUser(updateU))
                {
                    return RedirectToAction("ProfileUser");
                }
                ModelState.AddModelError("", "Update Error");
            }
            return View();
        }

        public ActionResult GetAirports()
        {
            List<string> airports = new List<string>();
            var airportsDB = FlightSearchDAO.GetAirports();
            foreach (var item in airportsDB)
            {
                airports.Add(string.Format("{0} ({1})", item.CityName, item.AirportID));
            }
            return Json(airports.ToArray(), JsonRequestBehavior.AllowGet);
            return Json(airports.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}