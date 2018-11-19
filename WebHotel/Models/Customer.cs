using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebHotel.Models;

namespace WebHotel.Models
{
    public class Customer
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public String Email { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Must be between 2 to 20 characters!")]
        [RegularExpression(@"^[a-zA-Z-']+$", ErrorMessage = "Can only consist of English letters, hyphens and apostrophes.")]
        public String Surname { get; set; }
        [Required]
        [Display(Name = "Given Name")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Must be between 2 to 20 characters!")]
        [RegularExpression(@"^[a-zA-Z-']+$", ErrorMessage = "Can only consist of English letters, hyphens and apostrophes.")]
        public String GivenName { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Postcode must be exactly 4 numeric characters!")]
        public String Postcode { get; set; }
        public ICollection<Booking> TheBookings { get; set; }
    }
}
