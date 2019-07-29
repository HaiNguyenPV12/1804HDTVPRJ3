using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Models.ars
{
    public class Dropdowns
    {
        public static IEnumerable<SelectListItem> Classes()
        {
            var list = new List<SelectListItem>()
            {
            new SelectListItem() {Value="E",Text="Economy"},
            new SelectListItem() {Value="B",Text="Business/Coach"},
            new SelectListItem() {Value="F",Text="First Class"}
            };
            return list;
        }
    }
}