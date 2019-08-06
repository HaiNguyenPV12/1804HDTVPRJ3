using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.ars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Controllers
{
    public partial class ARSController : Controller
    {
        //public ActionResult ReScheduleTest(long id)
        //{
        //    return Content(ReScheduleDAO.GetSearchParamByOrder(id));
        //}

        public ActionResult ReSchedule(long? id)
        {
            if (id != null)
            {
                // Check if user is logged in or not
                if (!isLoggedIn())
                {
                    Session["GotoPayment"] = "/ars/reschedule?id=" + id;
                    return RedirectToAction("Login");
                }

                var orderid = Convert.ToInt64(id);
                Order order = PaymentDAO.GetOrder(orderid);
                if (order != null && order.UserID == Session["user"].ToString())
                {
                    ViewBag.OrderID = orderid;
                    return View(order);
                }
                ViewBag.Message = "You cannot re-schedule this order.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ReSchedule(FormCollection frmReschedule)
        {
            var searchParams = (FlightSearch)Session["searchParams"];
            searchParams.DepartureTime = DateTime.Parse(frmReschedule["Departuretime"].ToString());
            if (frmReschedule["ReturnDepartureTime"] != null)
            {
                searchParams.ReturnDepartureTime = DateTime.Parse(frmReschedule["ReturnDepartureTime"].ToString());
            }

            Session["reschedule"] = long.Parse(frmReschedule["OrderID"].ToString());
            int totalPassenger = searchParams.Adult + searchParams.Children;
            Session["totalPassenger"] = totalPassenger;
            //return RedirectToAction("FlightList", searchParams);
            return Content("");
        }

        public ActionResult PaymentReSchedule(string FNo, string ReFNo)
        {
            // Check if searchParams exists in session (mean user come here from search)
            // If not exist in session, return to Index with error message
            if (Session["searchParams"] != null)
            {
                // Check if user is not logged in, redirect to login page
                // And save current payment url to back again after successfully logged in 
                if (Session["user"] == null)
                {
                    Session["GotoPayment"] = string.Format("/ars/paymentreschedule?FNo=" + FNo + "&ReFNo=" + ReFNo);
                    TempData["loginM"] = "Please login to process your payment.";
                    return RedirectToAction("Login");
                }

                // Whether it's oneway or round trip, it's always have FNo 
                // (Oneway or First trip or array of flight with stop)
                if (!string.IsNullOrEmpty(FNo))
                {
                    var searchParam = (FlightSearch)Session["searchParams"];
                    List<string> FNos = new List<string>();
                    FNos.AddRange(FNo.Split(','));
                    ViewBag.FNo = FNo;
                    ViewBag.FNos = FNos;

                    if (!string.IsNullOrEmpty(ReFNo))
                    {
                        ViewBag.ReFNo = ReFNo;
                    }
                    ViewBag.PeopleNum = searchParam.Adult + searchParam.Children + searchParam.Senior;
                    ViewBag.AdultNum = searchParam.Adult + searchParam.Senior;
                    ViewBag.ChildNum = searchParam.Children;
                    ViewBag.Class = searchParam.Class;
                    return View();
                }
            }
            TempData["errorM"] = "There was an error executing your requests. Please try again";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PaymentReSchedule(FormCollection frmPayment)
        {
            // Prepare model object
            Payment objP = new Payment();
            List<Passenger> objPaList = new List<Passenger>();
            objP.OldOrderID = long.Parse(frmPayment["OldOrderID"].ToString());
            objP.FNo1 = frmPayment["FNo[1]"];
            objP.FNo2 = frmPayment["FNo[2]"];
            objP.ReFNo = frmPayment["ReFNo"];
            objP.PeopleNum = int.Parse(frmPayment["PeopleNum"]);
            objP.AdultNum = int.Parse(frmPayment["AdultNum"]);
            objP.ChildNum = int.Parse(frmPayment["ChildNum"]);
            objP.Class = frmPayment["Class"];
            objP.Total = double.Parse(frmPayment["Total"]);
            objP.CCNo = frmPayment["CCNo"];
            objP.CVV = frmPayment["CVV"];

            for (int i = 0; i < objP.PeopleNum; i++)
            {
                Passenger objPa = new Passenger();
                objPa.Firstname = frmPayment["Firstname" + i];
                objPa.Lastname = frmPayment["Lastname" + i];
                objPa.Sex = Convert.ToBoolean(int.Parse(frmPayment["Sex" + i]));
                objPa.Age = int.Parse(frmPayment["Age" + i]);
                objPa.PassportNo = frmPayment["PassportNo" + i];
                if (frmPayment["Service" + i] != null)
                {
                    objPa.Service = frmPayment["Service" + i].Split(',');
                }
                else
                {
                    objPa.Service = new string[0];
                }

                objPaList.Add(objPa);
            }
            objP.Passengers = objPaList;
            objP.UserID = Session["user"].ToString();

            // Send to DAO to process data
            string s = "";
            s = ReScheduleDAO.ProcessReschedule(objP);
            return Content(s);
        }

        bool isLoggedIn()
        {
            if (Session["user"] != null)
            {
                return true;
            }
            return false;
        }
    }
}