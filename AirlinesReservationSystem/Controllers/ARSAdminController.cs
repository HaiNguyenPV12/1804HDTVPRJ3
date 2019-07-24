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
        public ActionResult Index()
        {
            if (IsLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Login");
        }

        public ActionResult Route()
        {
            if (IsLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Login");
        }

        // ================ LOGIN ==================
        public ActionResult Login()
        {
            if (!IsLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Index");
        }

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
        public ActionResult Employee()
        {
            if (IsAdminLoggedIn())
            {
                return View();
            }
            return RedirectToAction("Index");
        }
        public ActionResult EmployeeList()
        {
            if (IsAdminLoggedIn())
            {
                return PartialView(EmployeeDAO.GetEmployeeList());
            }
            return Content("");
        }

        public ActionResult EmployeeDelete(string id)
        {
            if (IsAdminLoggedIn())
            {
                if (EmployeeDAO.DeleteEmployee(id))
                {
                    return Content("OK");
                }
            }
            return Content("Error");
        }

        public ActionResult EmployeeAdd()
        {
            if (IsAdminLoggedIn())
            {
                return PartialView();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
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

        public bool IsLoggedIn()
        {
            if (Session["employee"] != null)
            {
                return true;
            }
            return false;
        }

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