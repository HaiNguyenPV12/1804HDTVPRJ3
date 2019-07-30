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
        IEnumerable<FlightResult> flightResults;
        int numFlightsPerPage = 5;

        IEnumerable<FlightResult> GetFlightList(FlightSearch flightSearch) => FlightSearchDAO.GetFlightResults(flightSearch);

        int GetPages(FlightSearch flightSearch)
        {
            flightResults = GetFlightList(flightSearch);
            int pages;
            int flights = flightResults.Count();
            int pagesM = flights % numFlightsPerPage;
            if (pagesM > 0) { pages = (flights / numFlightsPerPage) + 1; }
            else { pages = flights / numFlightsPerPage; }
            return pages;
        }

        IEnumerable<FlightResult> GetFlightsForPage(int page)
        {
            if (page <= 0) page = 1;
            var model = flightResults.OrderBy(item => item.FlightVM.BasePrice).Skip((page - 1) * numFlightsPerPage).Take(numFlightsPerPage);
            return model;
        }
    }
}