using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models.ars
{
    public class UsersDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static User GetUser(string UserID) => db.User.Find(UserID);

        public static IEnumerable<User> GetUserList()
        {
            db = new AirlineDBEntities();
            return db.User;
        }

        public static User CheckLogin(string UserID, string Password) => db.User.Where(e => e.UserID.Trim() == UserID && e.Password == Password).FirstOrDefault();

        public static bool AddUser(User newU)
        {
            if (GetUser(newU.UserID) == null)
            {
                newU.Skymiles = 0;
                db.User.Add(newU);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public static bool UpdateUser(User updateU)
        {
            var e = GetUser(updateU.UserID);
            if (e != null)
            {
                e.Password = updateU.Password;
                e.FirstName = updateU.FirstName;
                e.LastName = updateU.LastName;
                e.Address = updateU.Address;
                e.Phone = updateU.Phone;
                e.Email = updateU.Email;
                e.Sex = updateU.Sex;
                e.Age = updateU.Age;
                e.CCNo = updateU.CCNo;
                e.PassportNo_ = updateU.PassportNo_;
                e.Skymiles = updateU.Skymiles;
                db.SaveChanges();
                return true;
            }
            return false;
        }

    }
}