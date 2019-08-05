﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.arsadmin;
using AirlinesReservationSystem.Models.ars;


namespace AirlinesReservationSystem.Controllers
{
    public class ARSAdminController : Controller
    {
        // GET: ARSAdmin
        public ActionResult Index() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Login");

        // ================ LOGIN ==================
        // LOGIN VIEW
        // Check in Session if user is not logged in, allow to go to Login page. If logged in, redirect to index.
        public ActionResult Login()
        {
            if (!IsLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Index");
        }

        // LOGIN PROCESS
        // Temporarily remove any validation other than ID and Password's validation. 
        // Then check if user exist or not in database. If exists, save to Session and redirect to index. If not, show error message. 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Employee emp)
        {
            ModelState.Remove("Firstname");
            ModelState.Remove("Lastname");
            ModelState.Remove("Address");
            ModelState.Remove("Phone");
            ModelState.Remove("Email");
            ModelState.Remove("Sex");
            ModelState.Remove("DoB");
            ModelState.Remove("IsActive");
            ModelState.Remove("Role");
            if (ModelState.IsValid)
            {
                var e = EmployeeDAO.CheckLogin(emp.EmpID, emp.Password);
                if (e != null)
                {
                    Session["employee"] = e;
                    if (Session["preUrl"] != null)
                    {
                        string preUrl = Session["preUrl"].ToString();
                        Session["preUrl"] = null;
                        return Redirect(preUrl);
                    }
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Invalid account!");
            }
            return View();
        }

        // ================ LOGOUT ==================
        // First, remove session and then redirect to index.
        public ActionResult Logout()
        {
            Session["employee"] = null;
            return RedirectToAction("Index");
        }

        // ================ ROUTE ==================
        // ROUTE's VIEW
        // Check in Session if user is logged in, allow to go to Route page. If not, redirect to login page.
        public ActionResult Route()
        {
            if (IsLoggedIn())
            {
                return View(RouteDAO.GetRouteList());
            }
            Session["preUrl"] = "/arsadmin/route";
            return RedirectToAction("Login");
        }

        // ROUTE DELETE's PROCESS
        // First check if user is logged in and Route ID is exists in database, allow to delete.
        public ActionResult RouteDelete(int id) => IsLoggedIn() && RouteDAO.DeleteRoute(id) ? Content("OK") : Content("Error");

        // ROUTE ADD'S VIEW
        public ActionResult RouteAdd()
        {
            if (IsLoggedIn())
            {
                return View();
            }
            Session["preUrl"] = "/arsadmin/routeadd";
            return RedirectToAction("Login");
        }

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
        public ActionResult RouteEdit(int id)
        {
            if (IsLoggedIn() && RouteDAO.GetRoute(id) != null)
            {
                return View(RouteDAO.GetRoute(id));
            }
            Session["preUrl"] = "/arsadmin/routeedit?id=" + id;
            return RedirectToAction("Login");
        }

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

        // ================ EMPLOYEE ==================
        // EMPLOYEE's VIEW
        public ActionResult Employee()
        {
            if (IsAdminLoggedIn())
            {
                return View(EmployeeDAO.GetEmployeeList());
            }
            Session["preUrl"] = "/arsadmin/employee";
            return RedirectToAction("Index");
        }

        // EMPLOYEE's LIST
        public ActionResult EmployeeList()
        {
            if (IsAdminLoggedIn())
            {
                return PartialView(EmployeeDAO.GetEmployeeList());
            }
            return Content("");
        }

        // EMPLOYEE DELETE's PROCESS
        public ActionResult EmployeeDelete(string id) => IsAdminLoggedIn() && EmployeeDAO.DeleteEmployee(id) ? Content("ok") : Content("Error");

        // EMPLOYEE ADD'S VIEW
        public ActionResult EmployeeAdd() => IsAdminLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

        public ActionResult EmployeeAddTemplate(int index)
        {
            ViewBag.AddIndex = index;
            return View();
        }
        // EMPLOYEE ADD'S PROCESS
        [HttpPost]
        public ActionResult EmployeeAdd(FormCollection frmEmployeeAdd)
        {
            string s = "";
            try
            {
                List<Employee> EmpList = new List<Employee>();
                Employee objE = new Employee();
                int i = 0;
                while (frmEmployeeAdd["EmpID[" + i + "]"] != null)
                {
                    objE = new Employee
                    {
                        EmpID = frmEmployeeAdd["EmpID[" + i + "]"],
                        Password = frmEmployeeAdd["Password[" + i + "]"],
                        Firstname = frmEmployeeAdd["Firstname[" + i + "]"],
                        Lastname = frmEmployeeAdd["Lastname[" + i + "]"],
                        Address = frmEmployeeAdd["Address[" + i + "]"],
                        Phone = frmEmployeeAdd["Phone[" + i + "]"],
                        Email = frmEmployeeAdd["Email[" + i + "]"],
                        Sex = Convert.ToBoolean(frmEmployeeAdd["Sex[" + i + "]"]),
                        DoB = DateTime.Parse(frmEmployeeAdd["DoB[" + i + "]"]),
                        Role = int.Parse(frmEmployeeAdd["Role[" + i + "]"]),
                        IsActive = true
                    };
                    EmpList.Add(objE);
                    i++;

                }

                // Check duplicate
                foreach (var item in EmpList)
                {
                    if (EmpList.Where(e => e.EmpID == item.EmpID).Count() > 1)
                    {
                        s += "Error: ID duplicated! Please check and try again.\n";
                        break;
                    }
                }

                // Check exist
                foreach (var item in EmpList)
                {
                    if (EmployeeDAO.GetEmployee(item.EmpID) != null)
                    {
                        s += "Error: ID \"" + item.EmpID + "\" exists.";
                    }
                }

                // Start adding
                if (s == "")
                {
                    string result = EmployeeDAO.AddEmployee(EmpList);
                    s += result;
                }
            }
            catch (Exception e)
            {
                s += e.Message + e.StackTrace;
            }

            return Content(s);
        }

        // EMPLOYEE EDIT'S VIEW
        public ActionResult EmployeeEdit(string id) => IsAdminLoggedIn() && EmployeeDAO.GetEmployee(id) != null ? View(EmployeeDAO.GetEmployee(id)) : (ActionResult)RedirectToAction("Index");

        // EMPLOYEE EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeEdit(Employee updateE)
        {
            if (ModelState.IsValid)
            {
                if (EmployeeDAO.UpdateEmployee(updateE))
                {
                    return RedirectToAction("Employee");
                }
                ModelState.AddModelError("", "Error updating this employee!");
            }
            return View(updateE);
        }
        //================ CUSTOMER ==================

        public ActionResult Customer() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");
        public ActionResult CustomerList() => IsLoggedIn() ? PartialView(UsersDAO.GetUserList()) : (ActionResult)Content("");

        public ActionResult CustomerEdit(string id) => IsLoggedIn() && UsersDAO.GetUser(id) != null ? PartialView(UsersDAO.GetUser(id)) : (ActionResult)RedirectToAction("Index");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerEdit(User updateU)
        {
            if (ModelState.IsValid)
            {
                if (UsersDAO.UpdateUser(updateU))
                {
                    return RedirectToAction("Customer");
                    //return Content("SuccessCustomer");
                }
                ModelState.AddModelError("", "Error updating this User!");
            }
            return PartialView();
        }


        // ================ SERVICE ==================
        // SERVICE's VIEW
        public ActionResult Service() => IsLoggedIn() ? View(ServiceDAO.GetServiceList()) : (ActionResult)RedirectToAction("Index");

        // SERVICE DELETE's PROCESS
        public ActionResult ServiceDelete(string id) => IsLoggedIn() && ServiceDAO.DeleteService(id) ? Content("OK") : Content("Error");

        // SERVICE ADD'S VIEW
        public ActionResult ServiceAdd()
        {
            if (IsLoggedIn())
            {
                ViewBag.ServiceID = ServiceDAO.GetNextServiceID();
                return View();
            }
            return RedirectToAction("Index");
        }

        // SERVICE ADD'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ServiceAdd(Service newS)
        {
            ModelState.Remove("IsServing");

            if (ModelState.IsValid)
            {
                newS.IsServing = true;
                if (ServiceDAO.AddService(newS))
                {
                    return RedirectToAction("Service");
                }
                else
                {
                    ModelState.AddModelError("", "This service ID is exists!");
                }
            }
            ViewBag.ServiceID = ServiceDAO.GetNextServiceID();
            return View();
        }

        // SERVICE EDIT'S VIEW
        public ActionResult ServiceEdit(string id)
        {
            if (IsLoggedIn())
            {
                var s = ServiceDAO.GetService(id);
                if (s != null)
                {
                    return View(s);
                }
                return RedirectToAction("Service");
            }
            return RedirectToAction("Index");
        }

        // SERVICE EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ServiceEdit(Service updateS)
        {
            if (ModelState.IsValid)
            {
                if (ServiceDAO.UpdateService(updateS))
                {
                    return RedirectToAction("Service");
                }
                else
                {
                    ModelState.AddModelError("", "Error updating this service! Please refresh and try again.");
                }
            }
            return View(updateS);
        }

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


        //public ActionResult FlightAdd(List<Flight> newF)
        //{
        //    ModelState.Remove("AvailSeatsF");
        //    ModelState.Remove("AvailSeatsE");
        //    ModelState.Remove("AvailSeatsB");
        //    ModelState.Remove("FlightTime");
        //    if (ModelState.IsValid)
        //    {
        //        string addResult = FlightDAO.AddFlight(newF);
        //        if (addResult == "ok")
        //        {
        //            return RedirectToAction("Flight");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", addResult);
        //        }
        //    }
        //    ViewBag.RouteData = RouteDAO.GetRouteList();
        //    return View();
        //}

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
                    item.AvailSeatsF = item.Route.Aircraft.FirstClassSeats;
                    item.AvailSeatsB = item.Route.Aircraft.BussinessSeats;
                    item.AvailSeatsE = item.Route.Aircraft.EconomySeats;
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


        // ================ ORDER ==================
        //ORDER VIEW
        public ActionResult Order() => IsLoggedIn() ? View(OrderDAO.GetOrderList()) : (ActionResult)RedirectToAction("Index");
        //Cancel Order
        public ActionResult CancelOrder(long id)
        {
            OrderDAO.CancelOrder(id);
            return RedirectToAction("Order");
        }
        //TICKET VIEW
        public ActionResult OrderDetails(long id) => IsLoggedIn() ? View(TicketDAO.GetTicketList(id)) : (ActionResult)RedirectToAction("Index");


        //TICKET EDIT
        public ActionResult TicketEdit(long id)
        {
            if (IsLoggedIn())
            {
                var s = TicketDAO.GetTicket(id);
                if (s != null)
                {
                    return View(s);
                }
                return RedirectToAction("OrderDetails");
            }
            return RedirectToAction("Index");
        }

        // TICKET EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TicketEdit(Ticket updateT)
        {
            if (ModelState.IsValid)
            {
                if (TicketDAO.UpdateTicket(updateT))
                {
                    return RedirectToAction("OrderDetails", new { id = updateT.OrderID });
                }
                else
                {
                    ModelState.AddModelError("", "Error updating this ticket! Please refresh and try again.");
                }
            }
            return View(updateT);
        }
        // ================ CHECK LOGIN ==================
        public bool IsLoggedIn() => Session["employee"] != null;

        public bool IsAdminLoggedIn()
        {
            Employee e = (Employee)Session["employee"];
            if (e != null)
            {
                if (e.Role == 0)
                {
                    return true;
                }
            }
            return false;
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