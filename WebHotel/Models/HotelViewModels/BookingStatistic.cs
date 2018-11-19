using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebHotel.Models.HotelViewModels
{
    public class BookingStatistic
    {
        [Display(Name = "Postcode")]
        public int PostCode { get; set; }
        [Display(Name = "Number of Customers")]
        public int CustomerCount { get; set; }
        [Display(Name = "Room ID")]
        public int RoomID { get; set; }
        [Display(Name = "Number of Bookings")]
        public int BookingCount { get; set; }
    }
}
