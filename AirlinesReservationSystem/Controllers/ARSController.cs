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
        // GET: Home
        public ActionResult Index()
        {
            if (Session["fid1"] != null) Session["fid1"] = null;
            if (Session["fid2"] != null) Session["fid2"] = null;
            return View();
        }

        [HttpPost]
        public ActionResult Index(FlightSearch flightSearch)
        {
            Session["searchParams"] = flightSearch;
            int totalPassenger = flightSearch.Adult + flightSearch.Children + flightSearch.Senior;
            int totalAdults = flightSearch.Adult + flightSearch.Senior;
            if (!flightSearch.IsRoundTrip) { ModelState.Remove("ReturnDepartureTime"); }
            if (ModelState.IsValid && totalPassenger > 0 && totalAdults > 0)
            {
                //Session["searchParams0"] = flightSearch;
                Session["totalPassenger"] = totalPassenger;
                return RedirectToAction("FlightList", flightSearch);
            }
            else
            {
                if (totalPassenger <= 0)
                    ModelState.AddModelError("", "Passengers numbers are invalid");
                if (totalAdults < flightSearch.Children && totalAdults <= 0)
                    ModelState.AddModelError("", "Children must be accompanied by adults");
                ModelState.AddModelError("", "Please enter required values.");
            }
            return View(flightSearch);
        }

        public ActionResult Login()
        {
            if (Session["user"] == null)
            {
                try
                {
                    //Get previous url to redirect after login
                    string url = this.Request.UrlReferrer.ToString();
                    Session["Goto"] = url;
                }
                catch (Exception)
                {
                    Session["Goto"] = "/"; //returns to home if nothing was found
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
            //Remove unnecessary validations for login
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

            //Check if login is valid if validation is successful
            if (ModelState.IsValid)
            {
                var user = UsersDAO.CheckLogin(login.UserID, login.Password);
                if (user != null)
                {
                    Session["user"] = user.UserID;
                    if (Session["GotoPayment"] != null)
                    {
                        string Goto = Session["GotoPayment"].ToString();
                        Session["GotoPayment"] = null;
                        return Redirect(Goto);
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

        //Resets the user Session and go back to home
        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["GotoPayment"] = null;
            return RedirectToAction("Index");
        }


        //Passing search parameters into view
        //public ActionResult FlightList(FlightSearch flightSearch, bool? isReselect)
        //{
        //    Session["fid1"] = null;

        //    //get original search parameters and rerun search query
        //    if (isReselect == true)
        //        flightSearch = (FlightSearch)Session["searchParams"];

        //    ViewBag.RoundTrip = flightSearch.IsRoundTrip;
        //    var model = FlightSearchDAO.GetFlightResults(flightSearch);
        //    Session["searchResultsFirstTrip"] = model;
        //    return View(model);
        //}

        public ActionResult FlightList(FlightSearch flightSearch, bool? isReselect, int? page)
        {
            Session["fid1"] = null;

            //get original search parameters and rerun search query
            if (isReselect == true || !TryValidateModel(flightSearch))
                flightSearch = (FlightSearch)Session["searchParams"];
            ViewBag.RoundTrip = flightSearch.IsRoundTrip;
            ViewBag.Pages = GetPages(flightSearch);
            //var model = FlightSearchDAO.GetFlightResults(flightSearch);
            Session["searchResultsFirstTrip"] = flightResults;
            if (page == null)
            {
                ViewBag.PageIndex = 1;
                return View(GetFlightsForPage(1));
            }
            ViewBag.PageIndex = page;
            return View(GetFlightsForPage(int.Parse(page.ToString())));
        }

        //Passing 1st trip and run a reverse search with return date
        public ActionResult FlightListReturn(string fid, int rid)
        {
            try
            {
                Session["fid1"] = fid;
                Session["fid2"] = null;
                FlightResult firstTrip = FlightSearchDAO.GetFlightResult(fid, rid);
                FlightSearch flightSearch = (FlightSearch)Session["searchParams"];
                var seatsLeft = firstTrip.FlightVM.AvailSeatsB + firstTrip.FlightVM.AvailSeatsF + firstTrip.FlightVM.AvailSeatsE;

                //interrupt check for available seats
                if (seatsLeft == 0)
                {
                    TempData["NoSeatsMessage"] = "Sorry, there are no more seats left for flight " + firstTrip.FlightVM.FNo;
                    return RedirectToAction("FlightList", flightSearch);
                }

                //create new search parameters in memory that references the original parameters (else changing the depatures will change session's values)
                FlightSearch flightSearchReturn = FlightSearchDAO.Copy(flightSearch);
                ViewBag.firstTrip = firstTrip;

                //flip departure and arrival and run search query
                var roundTripEnd = flightSearchReturn.Departure;
                var roundTripStart = flightSearchReturn.Destination;
                flightSearchReturn.Departure = roundTripStart;
                flightSearchReturn.Destination = roundTripEnd;
                flightSearchReturn.DepartureTime = flightSearch.ReturnDepartureTime;
                var model = FlightSearchDAO.GetFlightResults(flightSearchReturn);

                //for debugging, making sure the original params are intact
                Session["searchParams"] = flightSearch;
                return View(model);
            }
            catch (Exception)
            {
                //returns to home if search reversal procedure fails at any point
                TempData["errorM"] = "There was an error executing your requests. Please try again";
                return RedirectToAction("Index");
            }
        }

        //Refine search details from results
        [HttpPost]
        public ActionResult FlightList(FlightSearchDetails flightSearchDetails)
        {
            return View();
        }

        //Returns view model of selected flight.
        public ActionResult FlightDetails(string fid, int rid)
        {
            var model = FlightSearchDAO.GetFlightResult(fid, rid);
            return View(model);
        }

        //public ActionResult TypeAheadDemo() => View();
        //public ActionResult JqueryUIDemo() => View();

        //Sign up view
        public ActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User newU)
        {
            if (ModelState.IsValid)
            {
                //check if user already exists
                if (UsersDAO.AddUser(newU))
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "UserID duplicated!!");
            }
            return View();
        }


        //User Details View
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


        //Edit User details view
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
                //if edit was successful
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
        public ActionResult Payment()
        {
            if (Session["searchParams"] != null)
            {
                var searchParam = (FlightSearch)Session["searchParams"];
                if (Session["fid1"] != null)
                {
                    if (Session["user"] == null)
                    {
                        Session["GotoPayment"] = string.Format("/ars/payment");
                        return RedirectToAction("Login");
                    }
                    ViewBag.FNo = Session["fid1"].ToString();
                    if (Session["fid2"] != null)
                    {
                        ViewBag.ReFNo = Session["fid2"].ToString();
                    }
                    ViewBag.PeopleNum = searchParam.Adult + searchParam.Children + searchParam.Senior;
                    ViewBag.AdultNum = searchParam.Adult + searchParam.Senior;
                    ViewBag.ChildNum = searchParam.Children;
                    ViewBag.Class = searchParam.Class;
                    return View();
                }
            }
            return RedirectToAction("Index");
        }

        // PAYMENT's PROCESS
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
            objP.AdultNum = int.Parse(frmPayment["AdultNum"]);
            objP.ChildNum = int.Parse(frmPayment["ChildNum"]);
            objP.Class = frmPayment["Class"];
            string[] Firstname = frmPayment["Firstname"].Split(',');
            string[] Lastname = frmPayment["Lastname"].Split(',');
            string[] SexStr = frmPayment["Sex"].Split(',');
            string[] PassportNo = frmPayment["PassportNo"].Split(',');
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

            for (int i = 0; i < objP.PeopleNum; i++)
            {
                Passenger objPa = new Passenger();
                objPa.Firstname = Firstname[i];
                objPa.Lastname = Lastname[i];
                objPa.Sex = Sex[i];
                objPa.Age = int.Parse(Age[i]);
                objPa.PassportNo = PassportNo[i];
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

        //---------------- PAYMENT RESULT ------------------
        // PAYMENT RESULT's VIEW
        public ActionResult PaymentResult(long? id)
        {
            if (id != null)
            {
                var p = PaymentDAO.GetOrder(long.Parse(id.ToString()));
                if (p != null)
                {
                    if (Session["user"] == null)
                    {
                        Session["GotoPayment"] = "/ars/paymentresult?id=" + id;
                        return RedirectToAction("Login");
                    }

                    ViewBag.Order = p;
                    ViewBag.TicketList = PaymentDAO.GetTicketList(p.OrderID);
                    return View(p);
                }
            }
            return RedirectToAction("PaymentSearch");
        }
        // BOOKING DETAIL's VIEW
        public ActionResult PaymentSearch()
        {
            if (Session["user"] != null)
            {
                return View();
            }
            Session["GotoPayment"] = "/ars/paymentsearch";
            return RedirectToAction("Login");
        }

        //---------------- ORDER PROCESS ------------------
        // PAY THE BLOCKED ORDER
        public ActionResult BlockingPayment(Int64 id)
        {
            PaymentDAO.BlockingOrderPaid(id);
            return RedirectToAction("PaymentResult", new { id = id });
        }
        // CANCELED THE PAID ORDER
        public ActionResult CancelPayment(long id)
        {
            PaymentDAO.CancelOrder(id);
            return RedirectToAction("PaymentResult", new { id = id });
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