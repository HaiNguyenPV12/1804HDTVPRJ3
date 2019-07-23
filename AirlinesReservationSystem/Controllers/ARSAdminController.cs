using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Controllers
{
    public class ARSAdminController : Controller
    {
        // GET: ARSAdmin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Route()
        {
            return View();
        }

        public int CheckLogin()
        {
            return 0;
        }
    }
}