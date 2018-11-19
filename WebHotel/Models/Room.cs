using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebHotel.Models;

namespace WebHotel.Models
{
    public class Room
    {
        [Display(Name = "Room ID")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Level")]
        [RegularExpression(@"^[G123]{1}$", ErrorMessage = " This field can only contain exactly one " +
            "character of ‘G’,‘1’, ‘2’, or ‘3’")]
        public string Level { get; set; }

        [Display(Name = "Number of beds")]
        [RegularExpression(@"^[123]{1}$", ErrorMessage = " Rooms can only consist of '1', '2' or '3' beds ")]
        public int BedCount { get; set; }

        [Range(50, 300, ErrorMessage = "Prices should be between $50 and $300")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Navigation properties
        public ICollection<Booking> TheBookings { get; set; }
    }
}
