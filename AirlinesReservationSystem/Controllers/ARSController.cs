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
            if (Session["searchParams"] != null) { Session["searchParams"] = null; }
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
            Session["rid1"] = null;

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
        public ActionResult FlightListReturn(string fid, int rid, int? page)
        {
            try
            {
                var ridParse = int.TryParse(rid.ToString(), out int ridParsed);
                if (string.IsNullOrEmpty(fid))
                    fid = (string)Session["fid1"];
                if (!ridParse)
                    rid = (int)Session["rid1"];
                Session["fid1"] = fid;
                Session["rid1"] = rid;
                Session["fid2"] = null;
                FlightResult firstTrip = FlightSearchDAO.GetFlightResult(fid, rid);
                FlightSearch flightSearch = (FlightSearch)Session["searchParams"];
                ViewBag.RoundTrip = flightSearch.IsRoundTrip;
                var seatsLeft = firstTrip.FlightVM.AvailSeatsB + firstTrip.FlightVM.AvailSeatsF + firstTrip.FlightVM.AvailSeatsE;

                //interrupt check for available seats
                if (seatsLeft == 0)
                {
                    TempData["NoSeatsMessage"] = "Sorry, there are no more seats left for flight " + firstTrip.FlightVM.FNo;
                    return RedirectToAction("FlightList", flightSearch);
                }

                //flip search query
                FlightSearch flightSearchReturn = ReverseFlightSearch(flightSearch);

                ViewBag.firstTrip = firstTrip;

                ViewBag.Pages = GetPages(flightSearchReturn);
                if (page == null)
                {
                    ViewBag.PageIndex = 1;
                    return View("FlightList", GetFlightsForPage(1));
                }
                ViewBag.PageIndex = page;

                //for debugging, making sure the original params are intact
                //Session["searchParams"] = flightSearch;

                return View("FlightList", GetFlightsForPage(int.Parse(page.ToString())));
            }
            catch (Exception)
            {
                //returns to home if search reversal procedure fails at any point
                TempData["errorM"] = "There was an error executing your requests. Please try again";
                return RedirectToAction("Index");
            }
        }

        public ActionResult FlightListWithStops()
        {
            Session["fid1"] = null;
            FlightSearch flightSearch = (FlightSearch)Session["searchParams"];
            ViewBag.RoundTrip = flightSearch.IsRoundTrip;
            IEnumerable<FlightResult> firstTrips = FlightSearchDAO.GetFlightResultsWithStops(flightSearch).OrderBy(item=>item.FlightVM.BasePrice);
            Session["firstTrips"] = firstTrips;
            if (firstTrips.Count() == 0)
            {
                TempData["errorM"] = "Could not find any connecting trips to " + flightSearch.Destination + ", Sorry.";
                return RedirectToAction("Index");
            }
            return View(firstTrips);
        }

        public ActionResult FlightListWithStops2(string fid)
        {
            FlightSearch flightSearch = (FlightSearch)Session["searchParams"];
            ViewBag.RoundTrip = flightSearch.IsRoundTrip;
            if (Session["firstTrips"] == null) { return RedirectToAction("Index"); }
            IEnumerable<FlightResult> secondTrips = FlightSearchDAO.SecondTripFromStop;
            Session["fid2"] = null;
            IEnumerable<FlightResult> firstTrips = (IEnumerable<FlightResult>)Session["firstTrips"];
            FlightResult firstTrip = firstTrips.Where(item => item.FlightVM.FNo == fid).FirstOrDefault();
            Session["firstTrip"] = firstTrip;
            var model = from s in secondTrips
                        where s.FlightVM.DepartureTime >= firstTrip.FlightVM.ArrivalTime
                        orderby s.FlightVM.BasePrice
                        select s;
            return View(model);
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
                ModelState.AddModelError("", "Register Error");
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
        public ActionResult Payment(string FNo, string ReFNo)
        {
            // Check if searchParams exists in session (mean user come here from search)
            // If not exist in session, return to Index with error message
            if (Session["searchParams"] != null)
            {
                // Check if user is not logged in, redirect to login page
                // And save current payment url to back again after successfully logged in 
                if (Session["user"] == null)
                {
                    Session["GotoPayment"] = string.Format("/ars/payment?FNo=" + FNo + "&ReFNo=" + ReFNo);
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

        // PAYMENT's PROCESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(FormCollection frmPayment)
        {
            // Prepare model object
            Payment objP = new Payment();
            List<Passenger> objPaList = new List<Passenger>();
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
            //string[] Firstname = frmPayment["Firstname"].Split(',');
            //string[] Lastname = frmPayment["Lastname"].Split(',');
            //string[] SexStr = frmPayment["Sex"].Split(',');
            //string[] PassportNo = frmPayment["PassportNo"].Split(',');

            //bool[] Sex = new bool[objP.PeopleNum];
            //int a = 0;
            //foreach (var item in SexStr)
            //{
            //    Sex[a++] = Convert.ToBoolean(int.Parse(item));
            //}
            //string[] Age = frmPayment["Age"].Split(',');

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

        public ActionResult FlightCalendar(string Departure, string Destination, int? month, int? year)
        {
            var objD1 = FlightCalendarDAO.GetAirport(Departure);
            var objD2 = FlightCalendarDAO.GetAirport(Destination);
            ViewBag.ErrorM = "";
            if (objD1 == null)
            {
                ViewBag.ErrorM += "Departure missing ";
            }
            if (objD2 == null)
            {
                ViewBag.ErrorM += "Destination missing ";
            }
            ViewBag.Month = month;
            ViewBag.Year = year;
            if (objD1 != null && objD2 != null)
            {
                var FlightList = FlightCalendarDAO.GetFlight(Departure, Destination);
                ViewBag.FlightList = FlightList;
            }

            return View();
        }
    }
}