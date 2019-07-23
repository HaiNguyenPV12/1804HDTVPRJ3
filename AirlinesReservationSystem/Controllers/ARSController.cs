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

        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(Login login)
        {
            if (ModelState.IsValid)
            {
                var user = login; //TODO get user from database and check password 
                Session["user"] = user;
                return RedirectToAction("Index"); //TODO redirect to previous page instead of home
            }
            ModelState.AddModelError("", "Invalid login information");
            return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult FlightList(FlightSearch flightSearch)
        {
            //var flights = from flightAndRoutes in db
            return View();
        }

        public ActionResult TypeAheadDemo() => View();
    }
}