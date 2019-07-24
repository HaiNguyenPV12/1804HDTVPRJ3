using System;
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

        public ActionResult Route() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Login");

        // ================ LOGIN ==================
        public ActionResult Login() => !IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

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
            ModelState.Remove("ROLE");
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
        public ActionResult Employee() => IsAdminLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");
        public ActionResult EmployeeList() => IsAdminLoggedIn() ? PartialView(EmployeeDAO.GetEmployeeList()) : (ActionResult)Content("");

        public ActionResult EmployeeDelete(string id) => IsAdminLoggedIn() && EmployeeDAO.DeleteEmployee(id) ? Content("OK") : Content("Error");

        public ActionResult EmployeeAdd() => IsAdminLoggedIn() ? PartialView() : (ActionResult)RedirectToAction("Index");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeAdd(Employee newE)
        {
            ModelState.Remove("IsActive");
            ModelState.Remove("ROLE");
            if (ModelState.IsValid)
            {
                newE.IsActive = true;
                newE.ROLE = 1;
                if (EmployeeDAO.AddEmployee(newE))
                {
                    return Content("Success");
                }
                ModelState.AddModelError("", "ID Exists!");
            }
            return PartialView();
        }

        public ActionResult EmployeeEdit(string id) => IsAdminLoggedIn() && EmployeeDAO.GetEmployee(id) != null ? PartialView(EmployeeDAO.GetEmployee(id)) : (ActionResult)RedirectToAction("Index");

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

        // ================ CHECK LOGIN ==================
        public bool IsLoggedIn() => Session["employee"] != null;

        public bool IsAdminLoggedIn()
        {
            Employee e = (Employee)Session["employee"];
            if (e != null)
            {
                if (e.ROLE == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}