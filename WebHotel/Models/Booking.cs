using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebHotel.Models;

namespace WebHotel.Models
{
    public class Booking
    {
        public int ID { get; set; }
        public int RoomID { get; set; }
        public String CustomerEmail { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CheckIn { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CheckOut { get; set; }

        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }
        public Room TheRoom { get; set; }
        public Customer TheCustomer { get; set; }

    }
}
