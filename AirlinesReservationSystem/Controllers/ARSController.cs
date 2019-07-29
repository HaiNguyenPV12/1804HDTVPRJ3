using AirlinesReservationSystem.Models;
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

        [HttpPost]
        public ActionResult Index(FlightSearch flightSearch)
        {
            Session["searchParams"] = flightSearch;
            int totalPassenger = flightSearch.Adult + flightSearch.Children + flightSearch.Senior;
            if (!flightSearch.IsRoundTrip) { ModelState.Remove("ReturnDepartureTime"); }
            if (ModelState.IsValid && totalPassenger > 0)
            {
                Session["searchParams"] = flightSearch;
                return RedirectToAction("FlightList", flightSearch);
            }
            else
            {
                if (totalPassenger <= 0)
                    ModelState.AddModelError("", "Passengers numbers are invalid");
                ModelState.AddModelError("", "Please enter required values");
            }
            return View(flightSearch);
        }

        public ActionResult Login()
        {
            if (Session["user"] == null)
            {
                try
                {
                    //ViewBag.Goto = Goto;
                    string url = this.Request.UrlReferrer.AbsolutePath;
                    Session["Goto"] = url;
                }
                catch (Exception)
                {
                    Session["Goto"] = "/";
                }
                return View();
            }
            return RedirectToAction("Index");
        }
        public ActionResult Test()
        {
            //ViewBag.Goto = Goto;
            return View();
        }

        [HttpPost]
        public ActionResult Login(User login)
        {
            ModelState.Remove("Firstname");
            ModelState.Remove("Lastname");
            ModelState.Remove("Address");
            ModelState.Remove("Phone");
            ModelState.Remove("Email");
            ModelState.Remove("Sex");
            ModelState.Remove("Age");
            ModelState.Remove("CCNo");
            ModelState.Remove("PassportNo_");
            ModelState.Remove("Skymiles");
            if (ModelState.IsValid)
            {
                var user = UsersDAO.CheckLogin(login.UserID, login.Password); //TODO get user from database and check password 
                if (user != null)
                {
                    Session["user"] = user.UserID;
                    if (Session["GotoPayment"] != null)
                    {
                        return Redirect(Session["GotoPayment"].ToString());
                    }
                    if (Session["Goto"] != null)
                    {
                        return Redirect(Session["Goto"].ToString());
                    }
                    return RedirectToAction("Index"); //TODO redirect to previous page instead of home
                }
                ModelState.AddModelError("", "Invalid login information");
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            return RedirectToAction("Index");
        }


        public ActionResult FlightList(FlightSearch flightSearch)
        {
            //TODO sub string to get Airport IDs
            ViewBag.RoundTrip = flightSearch.IsRoundTrip;
            var model = FlightSearchDAO.GetFlightResults(flightSearch);
            Session["searchResultsFirstTrip"] = model;
            return View(model);
        }

        public ActionResult FlightListReturn(string fid, int rid)
        {
            try
            {
                FlightResult firstTrip = FlightSearchDAO.GetFlightResult(fid, rid);
                FlightSearch flightSearch = (FlightSearch)Session["searchParams"];
                var seatsLeft = firstTrip.FlightVM.AvailSeatsB + firstTrip.FlightVM.AvailSeatsF + firstTrip.FlightVM.AvailSeatsE;
                if (seatsLeft == 0)
                {
                    TempData["NoSeatsMessage"] = "Sorry, there are no more seats left for flight " + firstTrip.FlightVM.FNo;
                    return RedirectToAction("FlightList", flightSearch);
                }
                ViewBag.firstTrip = firstTrip;
                var roundTripEnd = flightSearch.Departure;
                var roundTripStart = flightSearch.Destination;
                flightSearch.Departure = roundTripStart;
                flightSearch.Destination = roundTripEnd;
                flightSearch.DepartureTime = flightSearch.ReturnDepartureTime;
                var model = FlightSearchDAO.GetFlightResults(flightSearch);
                return View(model);
            }
            catch (Exception)
            {
                TempData["errorM"] = "There was an error executing your requests. Please try again";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult FlightList(FlightSearchDetails flightSearchDetails)
        {
            return View();
        }

        public ActionResult FlightDetails(string fid, int rid)
        {
            var model = FlightSearchDAO.GetFlightResult(fid, rid);
            return View(model);
        }

        public ActionResult TypeAheadDemo() => View();
        public ActionResult JqueryUIDemo() => View();

        public ActionResult Register() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User newU)
        {
            if (ModelState.IsValid)
            {
                if (UsersDAO.AddUser(newU))
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Register Error");
            }
            return View();
        }

        public ActionResult ProfileUser()
        {
            if (Session["user"] != null)
            {
                var model = UsersDAO.GetUser(Session["user"].ToString());
                if (model != null)
                {
                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult EditUser()
        {
            if (Session["user"] != null)
            {
                var model = UsersDAO.GetUser(Session["user"].ToString());
                if (model != null)
                {
                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User updateU)
        {
            if (ModelState.IsValid)
            {
                if (UsersDAO.UpdateUser(updateU))
                {
                    return RedirectToAction("ProfileUser");
                }
                ModelState.AddModelError("", "Update Error");
            }
            return View();
        }

        //---------------- PAYMENT ------------------
        // PAYMENT's VIEW
        public ActionResult Payment(string FNo, string ReFNo, int PeopleNum)
        {
            if (Session["user"] != null)
            {
                ViewBag.FNo = FNo;
                ViewBag.RFNo = ReFNo;
                ViewBag.PeopleNum = PeopleNum;
                return View();
            }
            Session["GotoPayment"] = string.Format("/ars/payment?FNo={0}&ReFNo={1}&PeopleNum={2}", FNo, ReFNo, PeopleNum);
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(FormCollection frmPayment)
        {
            // Prepare model object
            Payment objP = new Payment();
            List<Passenger> objPaList = new List<Passenger>();
            objP.FNo = frmPayment["FNo"];
            objP.ReFNo = frmPayment["ReFNo"];
            objP.PeopleNum = int.Parse(frmPayment["PeopleNum"]);
            string[] Firstname = frmPayment["Firstname"].Split(',');
            string[] Lastname = frmPayment["Lastname"].Split(',');
            string[] SexStr = frmPayment["Sex"].Split(',');
            string[] PassportNo = frmPayment["PassportNo"].Split(',');
            string[] Class = frmPayment["Class"].Split(',');
            string[] ReClass = null;
            if (frmPayment["ReClass"] != null)
            {
                ReClass = frmPayment["ReClass"].Split(',');
            }
            bool[] Sex = new bool[objP.PeopleNum];
            int a = 0;
            foreach (var item in SexStr)
            {
                Sex[a++] = Convert.ToBoolean(int.Parse(item));
            }
            string[] Age = frmPayment["Age"].Split(',');

            //string[][] Service = new string[objP.PeopleNum][];
            for (int i = 0; i < objP.PeopleNum; i++)
            {
                Passenger objPa = new Passenger();
                objPa.Firstname = Firstname[i];
                objPa.Lastname = Lastname[i];
                objPa.Sex = Sex[i];
                objPa.Age = int.Parse(Age[i]);
                objPa.PassportNo = PassportNo[i];
                objPa.Class = Class[i];
                if (frmPayment["ReClass"] != null)
                {
                    objPa.ReClass = ReClass[i];
                }
                if (frmPayment["Service" + (i + 1)] != null)
                {
                    objPa.Service = frmPayment["Service" + (i + 1)].Split(',');
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
            if (string.IsNullOrEmpty(frmPayment["IsBlock"]))
            {
                s = PaymentDAO.ProcessPayment(objP, false);
            }
            else
            {
                s = PaymentDAO.ProcessPayment(objP, true);
            }

            return Content(s);
        }

        public ActionResult PaymentResult(Int64 id)
        {
            return View(PaymentDAO.GetOrder(id));
        }

        public ActionResult GetAirports()
        {
            List<string> airports = new List<string>();
            var airportsDB = FlightSearchDAO.GetAirports();
            foreach (var item in airportsDB)
            {
                airports.Add(string.Format("{0} ({1})", item.CityName, item.AirportID));
            }
            return Json(airports.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}