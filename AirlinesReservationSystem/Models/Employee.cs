//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AirlinesReservationSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Employee
    {
        [Required]
        [Key]
        [Display(Name ="Employee ID")]
        public string EmpID { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public bool Sex { get; set; }
        [Required]
        public System.DateTime DoB { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public int ROLE { get; set; }
    }
}
