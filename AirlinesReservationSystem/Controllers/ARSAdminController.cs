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
        //CUSTOMER VIEW
        public ActionResult Customer() => IsLoggedIn() ? View() : (ActionResult)RedirectToAction("Index");

        //CUSTOMER LIST
        public ActionResult CustomerList() => IsLoggedIn() ? PartialView(UsersDAO.GetUserList()) : (ActionResult)Content("");

        //CUSTOMER EDIT
        public ActionResult CustomerEdit(string id) => IsLoggedIn() && UsersDAO.GetUser(id) != null ? PartialView(UsersDAO.GetUser(id)) : (ActionResult)RedirectToAction("Index");

        //CUSTOMER EDIT'S PROCESS
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


        // ================ ORDER ==================
        //ORDER VIEW
        public ActionResult Order() => IsLoggedIn() ? View(OrderDAO.GetOrderList()) : (ActionResult)RedirectToAction("Index");


        //CANCELLED ORDER
        public ActionResult CancelOrder(long id)
        {
            OrderDAO.CancelOrder(id);
            return RedirectToAction("Order");
        }
        //ORDER DETAILS VIEW
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

        // TICKETEDIT'S PROCESS
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

        public ActionResult AddDBRoute()
        {
            InsertDAO.InsertRoutes();
            return RedirectToAction("Index");
        }

        public ActionResult AddDBFlight()
        {
            InsertDAO.InsertFlight();
            return RedirectToAction("Index");
        }

        public ActionResult AddDBFlightToRoute(int RNo, int month)
        {
            InsertDAO.InsertFlightsIntoRoute(RNo, month);
            return RedirectToAction("Index");
        }
    }
}