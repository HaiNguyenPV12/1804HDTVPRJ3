using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.bank
{
    public class BankDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static IEnumerable<CreditCard> GetCreditCards() => db.CreditCard;



        public static CreditCard GetCreditCard(string cardNo) => db.CreditCard.FirstOrDefault(item => item.CCNo == cardNo);

        public static bool AddCreditCard(CreditCard creditCard)
        {
            try
            {
                var cCardSearch = GetCreditCard(creditCard.CCNo);
                if (cCardSearch == null)
                {
                    db.CreditCard.Add(creditCard);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception) { return false; }
        }

        public static bool EditCreditCard(CreditCard creditCard)
        {
            try
            {
                var c = GetCreditCard(creditCard.CCNo);
                if (c != null)
                {
                    c.CVV = creditCard.CVV;
                    c.OwnerName = creditCard.OwnerName;
                    c.Balance = creditCard.Balance;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception) { return false; }
        }

        public static bool DeleteCreditCard(string cardNo)
        {
            try
            {
                var c = GetCreditCard(cardNo);
                if (c != null)
                {
                    db.CreditCard.Remove(c);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception) { return false; }
        }
    }
}