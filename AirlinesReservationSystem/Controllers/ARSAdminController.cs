﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirlinesReservationSystem.Models;

namespace AirlinesReservationSystem.Controllers
{
    public class ARSAdminController : Controller
    {

        // GET: ARSAdmin
        public ActionResult Index() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Login");

        // ================ ROUTE ==================
        public ActionResult Route() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Login");


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


        // ================ EMPLOYEE ==================
        // EMPLOYEE VIEW
        public ActionResult Employee() => IsAdminLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

        // EMPLOYEE LIST
        public ActionResult EmployeeList() => IsAdminLoggedIn() ? PartialView(EmployeeDAO.GetEmployeeList()) : (ActionResult)Content("");

        // EMPLOYEE DELETE PROCESS
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

        // ================ SERVICE ==================
        // SERVICE VIEW
        public ActionResult Service() => IsLoggedIn() ? View(ServiceDAO.GetServiceList()) : (ActionResult)RedirectToAction("Index");

        // SERVICE DELETE PROCESS
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