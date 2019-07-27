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

    public partial class Flight
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Flight()
        {
            this.Ticket = new HashSet<Ticket>();
        }
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(10, MinimumLength = 3)]
        [Display(Name ="Flight No.")]
        public string FNo { get; set; }

        [Required]
        public int RNo { get; set; }

        public Nullable<int> AvailSeatsF { get; set; }

        public Nullable<int> AvailSeatsB { get; set; }

        [Required]
        public int AvailSeatsE { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString ="{0:dd-MM-yyyy HH:mm}")]
        public System.DateTime DepartureTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}")]
        public System.DateTime ArrivalTime { get; set; }

        [Required]
        public double FlightTime { get; set; }

        [Required]
        [Range(1,50000)]
        public double BasePrice { get; set; }

        public virtual Route Route { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ticket> Ticket { get; set; }
    }
}
