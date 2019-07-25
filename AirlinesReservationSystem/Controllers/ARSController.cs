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
                    Session["user"] = user;
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
            if (flightSearch.IsRoundTrip)
            {
                ViewBag.RoundTrip = true;
            }
            var model = FlightSearchDAO.GetFlightResults(flightSearch);
            return View(model);
        }

        public ActionResult FlightListReturn(FlightSearch flightSearch)
        {
            var roundTripEnd = flightSearch.Departure;
            var roundTripStart = flightSearch.Destination;
            flightSearch.Departure = roundTripStart;
            flightSearch.Destination = roundTripEnd;
            flightSearch.DepartureTime = flightSearch.ReturnDepartureTime;
            var model = FlightSearchDAO.GetFlightResults(flightSearch);
            return View(model);
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
    }
}