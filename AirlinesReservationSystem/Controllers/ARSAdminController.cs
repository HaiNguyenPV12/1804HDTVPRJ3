using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.ars;
using AirlinesReservationSystem.Models.arsadmin;

namespace AirlinesReservationSystem.Controllers
{
    public class ARSAdminController : Controller
    {
        // GET: ARSAdmin
        public ActionResult Index() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Login");

        // ================ LOGIN ==================
        // LOGIN VIEW
        public ActionResult Login() => !IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");
        // LOGIN PROCESS
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
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Invalid account!");
            }
            return View();
        }

        // ================ LOGOUT ==================
        public ActionResult Logout()
        {
            Session["employee"] = null;
            return RedirectToAction("Index");
        }

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

        // ROUTE ADD'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RouteAdd(Route newR)
        {
            if (ModelState.IsValid)
            {
                string addResult = RouteDAO.AddRoute(newR);
                if (addResult == "ok")
                {
                    return RedirectToAction("Route");
                }
                else
                {
                    ModelState.AddModelError("", addResult);
                }
            }
            return View();
        }
        // EMPLOYEE EDIT'S VIEW
        public ActionResult RouteEdit(int id) => IsLoggedIn() && RouteDAO.GetRoute(id) != null ? View(RouteDAO.GetRoute(id)) : (ActionResult)RedirectToAction("Index");

        // EMPLOYEE EDIT'S PROCESS
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
        public ActionResult Employee() => IsAdminLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

        // EMPLOYEE's LIST
        public ActionResult EmployeeList() => IsAdminLoggedIn() ? PartialView(EmployeeDAO.GetEmployeeList()) : (ActionResult)Content("");

        // EMPLOYEE DELETE's PROCESS
        public ActionResult EmployeeDelete(string id) => IsAdminLoggedIn() && EmployeeDAO.DeleteEmployee(id) ? Content("OK") : Content("Error");

        // EMPLOYEE ADD'S VIEW
        public ActionResult EmployeeAdd() => IsAdminLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

        public ActionResult EmployeeAddTemplate(int index)
        {
            ViewBag.AddIndex = index;
            return View();
        }
        // EMPLOYEE ADD'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeAdd(List<Employee> newEs)
        {
            ModelState.Remove("IsActive");
            ModelState.Remove("Role");
            foreach (var e in newEs)
            {
                if (ModelState.IsValid)
                {
                    e.IsActive = true;
                    e.Role = 1;
                    if (EmployeeDAO.AddEmployee(e))
                    {
                        if (e == newEs.Last())
                            return Content("Success");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cannot add " + e.EmpID);
                        break;
                    }
                }
            }

            return View();
        }

        // EMPLOYEE EDIT'S VIEW
        public ActionResult EmployeeEdit(string id) => IsAdminLoggedIn() && EmployeeDAO.GetEmployee(id) != null ? PartialView(EmployeeDAO.GetEmployee(id)) : (ActionResult)RedirectToAction("Index");

        // EMPLOYEE EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeEdit(Employee updateE)
        {
            if (ModelState.IsValid)
            {
                if (EmployeeDAO.UpdateEmployee(updateE))
                {
                    return Content("Success");
                }
                ModelState.AddModelError("", "Error updating this employee!");
            }
            return PartialView();
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

        // ROUTE ADD'S VIEW
        public ActionResult FlightAdd()
        {
            if (IsLoggedIn())
            {
                ViewBag.RouteData = RouteDAO.GetRouteList();
                return View();
            }
            return RedirectToAction("Index");
        }

        // ROUTE ADD'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FlightAdd(Flight newF)
        {
            if (ModelState.IsValid)
            {
                string addResult = FlightDAO.AddFlight(newF);
                if (addResult == "ok")
                {
                    return RedirectToAction("Flight");
                }
                else
                {
                    ModelState.AddModelError("", addResult);
                }
            }
            ViewBag.RouteData = RouteDAO.GetRouteList();
            return View();
        }
        // EMPLOYEE EDIT'S VIEW
        public ActionResult FlightEdit(string id) => IsLoggedIn() && FlightDAO.GetFlight(id) != null ? View(FlightDAO.GetFlight(id)) : (ActionResult)RedirectToAction("Index");

        // EMPLOYEE EDIT'S PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FlightEdit(Route updateF)
        {
            if (ModelState.IsValid)
            {
                var editResult = RouteDAO.UpdateRoute(updateF);
                if (editResult == "ok")
                {
                    return RedirectToAction("Route");
                }
                ModelState.AddModelError("", editResult);
            }
            return View(updateF);
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


    }
}