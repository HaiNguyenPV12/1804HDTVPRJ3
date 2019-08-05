using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.arsadmin;
using AirlinesReservationSystem.Models.ars;

namespace AirlinesReservationSystem.Controllers
{
    public partial class ARSAdminController : Controller
    {
        // ================ ROUTE ==================
        // ROUTE's VIEW
        public ActionResult Route()
        {
            if (IsLoggedIn())
            {
                return View(RouteDAO.GetRouteList());
            }
            return RedirectToAction("Index");
        }

        // ROUTE DELETE's PROCESS
        public ActionResult RouteDelete(int id) => IsLoggedIn() && RouteDAO.DeleteRoute(id) ? Content("OK") : Content("Error");

        // ROUTE ADD'S VIEW
        public ActionResult RouteAdd() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

        //ROUTE ADD TEMPLATE
        public ActionResult RouteAddTemplate(int index)
        {
            ViewBag.Index = index;
            return View();
        }

        // ROUTE ADD'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RouteAdd(List<Route> newRoutes)
        {
            //init variables for validation purposes
            bool invalid = false, invalid2 = false;
            List<Route> routeToSkip = new List<Route>();
            int total = newRoutes.Count;
            List<string> added = new List<string>();

            //check list for any invalid routes (if departure = destination)
            foreach (var item in newRoutes)
            {
                if (item.Departure == item.Destination)
                {
                    ModelState.AddModelError("", string.Format("Error at: {0} with plane {1}. Arrival must be different than Departure", item.RAirline, item.RAircraft));
                    invalid2 = true;
                    //assign route as invalid
                    routeToSkip.Add(item);
                }
            }
            if (ModelState.IsValid || invalid == false)
            {
                bool skip = false;
                foreach (var item in newRoutes)
                {
                    foreach (var itemToSkip in routeToSkip)
                    {
                        if (item.Equals(itemToSkip)) { skip = true; }
                    }

                    //skip if item in original list equals to any in designated invalid routes, reset skip check
                    if (skip) { skip = false; continue; }

                    //perform add procedure. If route is invalid, add feedback message as validation error,
                    if (!RouteDAO.AddRoute(item))
                    {
                        ModelState.AddModelError("", string.Format("Could not add {0} with plane {1} going from {2} to {3}", item.RAirline, item.RAircraft, item.Departure, item.Destination));
                        invalid = true;
                        total--;
                    }
                    //else add to success strings to display in case invalid validation occurs
                    else
                        added.Add(string.Format("{0}({1}) : {2} - {3}", item.RAirline, item.RAircraft, item.Departure, item.Destination));
                }

                //if everything is inserted successfully, return to list
                if (!invalid && !invalid2)
                    return RedirectToAction("Route");

                //if there were errors, resets page and notify any inserted routes
                if (added.Count > 0)
                {
                    ModelState.AddModelError("", "Routes Added: ");
                    foreach (var item in added) { ModelState.AddModelError("", item); }
                }
            }
            return View();
        }


        // ROUTE EDIT'S VIEW
        public ActionResult RouteEdit(int id) => IsLoggedIn() && RouteDAO.GetRoute(id) != null ? View(RouteDAO.GetRoute(id)) : (ActionResult)RedirectToAction("Index");

        // ROUTE EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RouteEdit(Route updateR)
        {
            if (ModelState.IsValid)
            {
                var editResult = RouteDAO.UpdateRoute(updateR);
                if (editResult == "ok")
                {
                    return RedirectToAction("Route");
                }
                ModelState.AddModelError("", editResult);
            }
            return View();
        }

        //===Get aircraft range for route adding===
        public int GetAircraftRange(string id)
        {
            var a = RouteDAO.GetAircraft(id);
            return a.Range;
        }

        //===Get distance between two airports===
        public int GetFlightDistance(string airport1, string airport2)
        {
            var d = FlightDistanceDAO.GetFlightDistance(airport1, airport2);
            return d == null ? 0 : d.Distance;
        }
    }
}