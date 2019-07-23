using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace AirlinesReservationSystem.Models
{
    public class EmployeeDAO
    {
        static AirlineDBEntities db = new AirlineDBEntities();

        public static Employee GetEmployee(string EmpID) { return db.Employee.FirstOrDefault(e => e.EmpID == EmpID); }

        public static IEnumerable<Employee> GetEmployeeList() { return db.Employee; }

        public static Employee CheckLogin(string EmpID, string Password)
        {
            return db.Employee.Where(e => e.EmpID.Trim() == EmpID && e.Password == Password).FirstOrDefault();
        }
    }

}