using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.arsadmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Controllers
{
    public partial class ARSAdminController : Controller
    {
        // ================ FLIGHT ==================
        // FLIGHT's VIEW
        public ActionResult Flight()
        {
            if (IsLoggedIn())
            {
                return View(FlightDAO.GetFlightList());
            }
            return RedirectToAction("Index");
        }

        // FLIGHT DELETE's PROCESS
        public ActionResult FlightDelete(string id) => IsLoggedIn() && FlightDAO.DeleteFlight(id) ? Content("OK") : Content("Error");

        // FLIFHT ADD'S VIEW
        public ActionResult FlightAdd()
        {
            if (IsLoggedIn())
            {
                //ViewBag.RouteData = RouteDAO.GetRouteList();
                return View();
            }
            return RedirectToAction("Index");
        }

        public ActionResult FlightAddTemplate(int index)
        {
            ViewBag.Index = index;
            return View();
        }

        // FLIGHT ADD'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FlightAdd(List<Flight> newFlights)
        {
            //init validation variables
            bool invalid = false;
            int total = newFlights.Count;
            List<string> added = new List<string>();
            ModelState.Remove("AvailSeatsF");
            ModelState.Remove("AvailSeatsE");
            ModelState.Remove("AvailSeatsB");
            ModelState.Remove("FlightTime");
            if (ModelState.IsValid)
            {
                foreach (var item in newFlights)
                {
                    //get aircraft of route
                    var route = RouteDAO.GetRoute(item.RNo);
                    var aircraft = RouteDAO.GetAircraft(route.RAircraft);

                    //assign default seatnumbers
                    if (aircraft.FirstClassSeats != null)
                        item.AvailSeatsF = aircraft.FirstClassSeats;
                    if (aircraft.BussinessSeats != null)
                        item.AvailSeatsB = aircraft.BussinessSeats;
                    item.AvailSeatsE = aircraft.EconomySeats;
                    var diff = item.ArrivalTime.Hour - item.DepartureTime.Hour;
                    item.FlightTime = int.Parse(diff.ToString());
                    if (!FlightDAO.AddFlight(item))
                    {
                        ModelState.AddModelError("", string.Format("Could not add flight {0}", item.FNo));
                        invalid = true;
                        total--;
                    }
                    else
                        added.Add(string.Format("{0}", item.FNo));
                }
            }

            if (!invalid)
            {
                return RedirectToAction("Flight");
            }

            if (added.Count > 0)
            {
                ModelState.AddModelError("", "Flights Added: ");
                foreach (var item in added) { ModelState.AddModelError("", item); }
            }

            return View();
        }

        // FLIGHT EDIT'S VIEW
        public ActionResult FlightEdit(string id)
        {
            if (IsLoggedIn())
            {
                var f = FlightDAO.GetFlight(id);
                if (f != null)
                {
                    ViewBag.RouteData = RouteDAO.GetRouteList();
                    return View(f);
                }
            }
            return RedirectToAction("Index");
        }

        // FLIGHT EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FlightEdit(Flight updateF)
        {
            ModelState.Remove("AvailSeatsF");
            ModelState.Remove("AvailSeatsE");
            ModelState.Remove("AvailSeatsB");
            ModelState.Remove("FlightTime");
            if (ModelState.IsValid)
            {
                var editResult = FlightDAO.UpdateFlight(updateF);
                if (editResult == "ok")
                {
                    return RedirectToAction("Flight");
                }
                ModelState.AddModelError("", editResult);
            }
            ViewBag.RouteData = RouteDAO.GetRouteList();
            return View(updateF);
        }
    }
}