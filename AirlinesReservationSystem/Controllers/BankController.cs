using AirlinesReservationSystem.Models;
using AirlinesReservationSystem.Models.bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirlinesReservationSystem.Controllers
{
    public class BankController : Controller
    {
        // GET: Bank
        public ActionResult Index()
        {
            return View(BankDAO.GetCreditCards());
        }

        public ActionResult AddCard() => View();

        [HttpPost]
        public ActionResult AddCard(CreditCard creditCard)
        {
            if (ModelState.IsValid && BankDAO.AddCreditCard(creditCard))
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Card number already exists.");
            return View();
        }

        public ActionResult EditCard(string cardNo) => View(BankDAO.GetCreditCard(cardNo));

        [HttpPost]
        public ActionResult EditCard(CreditCard creditCard)
        {
            if (ModelState.IsValid && BankDAO.EditCreditCard(creditCard))
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "There were errors editing account. Ensure that account still exists.");
            return View();
        }

        public ActionResult DeleteCard(string cardNo)
        {
            BankDAO.DeleteCreditCard(cardNo);
            return RedirectToAction("Index");
        }
    }
}