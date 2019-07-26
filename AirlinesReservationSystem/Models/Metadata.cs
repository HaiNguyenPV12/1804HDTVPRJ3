using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AirlinesReservationSystem.Models
{
    [MetadataType(typeof(EmployeeMetaData))]
    public partial class Employee
    {
        public class EmployeeMetaData
        {
            [Required]
            [Key]
            [Display(Name = "Employee ID")]
            [StringLength(50)]
            public string EmpID { get; set; }

            [Required]
            [StringLength(256)]
            public string Password { get; set; }

            [Required]
            [StringLength(50)]
            public string Firstname { get; set; }

            [Required]
            [StringLength(50)]
            public string Lastname { get; set; }

            [Required]
            [StringLength(200)]
            public string Address { get; set; }

            [Required]
            [Phone]
            public string Phone { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public bool Sex { get; set; }

            [Required]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
            [Display(Name = "Birthday")]
            public DateTime DoB { get; set; }

            [Required]
            [Display(Name = "Active")]
            public bool IsActive { get; set; }

            [Required]
            public int Role { get; set; }
        }
    }

    [MetadataType(typeof(ServiceMetaData))]
    public partial class Service
    {
        public class ServiceMetaData
        {
            [Required]
            [Key]
            [RegularExpression("S\\d{2}")]
            [Display(Name = "Service ID")]
            public string ServiceID { get; set; }

            [Required]
            [StringLength(50, MinimumLength = 3)]
            [Display(Name = "Service Name")]
            public string ServiceName { get; set; }

            [Display(Name = "Service Details")]
            public string ServiceDetails { get; set; }

            [Required]
            [Display(Name = "Service Fee")]
            [Range(5, 10000)]
            public double ServiceFee { get; set; }

            [Required]
            [Display(Name = "Service Active")]
            public bool IsServing { get; set; }
        }
    }
}