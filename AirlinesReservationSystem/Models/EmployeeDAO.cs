﻿using System;
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

        public static IEnumerable<Employee> GetEmployeeList() { return db.Employee.OrderByDescending(e=>e.IsActive); }

        public static Employee CheckLogin(string EmpID, string Password)
        {
            return db.Employee.Where(e => e.EmpID.Trim() == EmpID && e.Password == Password).FirstOrDefault();
        }

        public static bool AddEmployee(Employee newE)
        {
            if (GetEmployee(newE.EmpID) == null)
            {
                db.Employee.Add(newE);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public static bool DeleteEmployee(string id)
        {
            var e = GetEmployee(id);
            if (e != null)
            {
                db.Employee.Remove(e);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public static bool UpdateEmployee(Employee updateE)
        {
            var e = GetEmployee(updateE.EmpID);
            if (e != null)
            {
                e.Password = updateE.Password;
                e.Firstname = updateE.Firstname;
                e.Lastname = updateE.Lastname;
                e.Address = updateE.Address;
                e.Phone = updateE.Phone;
                e.Sex = updateE.Sex;
                e.IsActive = updateE.IsActive;
                e.DoB = updateE.DoB;
                e.Email = updateE.Email;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }

}