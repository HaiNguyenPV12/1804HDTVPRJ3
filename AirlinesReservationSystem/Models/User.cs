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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Order = new HashSet<Order>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [StringLength(50)]
        public string UserID { get; set; }

        [Required]
        [StringLength(256)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [StringLength(12)]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number have from 10 to 11 digits")]
        public string Phone { get; set; }

        [Required]
        [StringLength(256)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public bool Sex { get; set; }

        [Required]
        [Range(0,150)]
        public int Age { get; set; }

        [Required]
        [Display(Name = "Credit Card Number")]
        [StringLength(16,MinimumLength =16, ErrorMessage = "Credit Card number must be have 16 digits")]
        public string CCNo { get; set; }

        [RegularExpression("[A-Z,0-9]{6,9}",ErrorMessage = "Passport error")]
        public string PassportNo_ { get; set; }

        [Required]
        
        public int Skymiles { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
    }
}
