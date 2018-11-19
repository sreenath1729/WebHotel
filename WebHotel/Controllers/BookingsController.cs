using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebHotel.Data;
using WebHotel.Models;
using WebHotel.Models.HotelViewModels;

namespace WebHotel.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings?sortOrder=xxx     
        [Authorize(Roles = "Customers, Admin")]
        public async Task<IActionResult> Index(string sortOrder)
        {
            //var applicationDbContext = _context.Purchase.Include(p => p.TheCustomer).Include(p => p.ThePizza);

            // Prepare the query for getting the entire list of bookings.
            // Convert the data type from Dbset<booking> to IQueryable<booking>
            var bookings = (IQueryable<Booking>)_context.Booking.Include(p => p.TheCustomer).Include(p => p.TheRoom).Where(p => p.CustomerEmail == User.Identity.Name);

            //Sort the bookings by specified order
            switch (sortOrder)
            {
                case "check_asc":
                    bookings = bookings.OrderBy(m => m.CheckIn);
                    break;
                case "check_desc":
                    bookings = bookings.OrderByDescending(m => m.CheckIn);
                    break;
                case "cost_asc":
                    bookings = bookings.OrderBy(m => m.Cost);
                    break;
                case "cost_desc":
                    bookings = bookings.OrderByDescending(m => m.Cost);
                    break;
            }

            // Deciding query string (sortOrder=xxx) to include in heading links for PizzaName, PizzaCount and TotalCost respectively.
            // They specify the next display order if a heading link is clicked.
            // Store them in ViewData dictionary to pass them to View
            ViewData["NextCheckOrder"] = sortOrder != "check_asc" ? "check_asc" : "check_desc";
            ViewData["NextCostOrder"] = sortOrder != "cost_asc" ? "cost_asc" : "cost_desc";

            // Access database to execute the query prepared above
            // Pass the returned booking list to View
            //return View(await applicationDbContext.AsNoTracking().ToListAsync());
            return View(await bookings.AsNoTracking().ToListAsync());
        }
        // GET: Bookings/BookingManagement
        [Authorize(Roles ="Admin")]
         public async Task<IActionResult> BookingManagement()
         {
              var bookings = (IQueryable<Booking>)_context.Booking.Include(p => p.TheCustomer).Include(p => p.TheRoom);
              

            // Access database to execute the query prepared above
            // Pass the returned booking list to View
            // return View(await applicationDbContext.AsNoTracking().ToListAsync());
            return View(await bookings.AsNoTracking().ToListAsync());
        
         }

        // Search Rooms
        // GET: Bookings/SearchRooms
        [Authorize(Roles = "Customers")]
        public IActionResult SearchRooms()
        {
            ViewBag.BedCountList = new SelectList(_context.Room, "BedCount", "BedCount");
            return View();
        }

        // SearchRooms
        // POST: Bookings/SearchRooms
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Customers")]
        public async Task<IActionResult> SearchRooms(RoomSearch roomSearch)
        {
            // prepare the parameters to be inserted into the query

            var totalBeds = new SqliteParameter("bedCount", roomSearch.BedCount);
            var cInDate = new SqliteParameter("checkIn", roomSearch.CheckIn);
            var cOutDate = new SqliteParameter("checkOut", roomSearch.CheckOut);

            var searchRoom = _context.Room.FromSql("select * from [Room]"
            + "where [Room].BedCount = @bedCount and [Room].ID not in " +
            "(select [Room].ID from [Room] inner join [Booking] on [Room].ID = [Booking].RoomID " +
            "where ([Booking].checkIn <= @checkIn and [Booking].checkOut >= @checkIn)" +
            "or ([Booking].checkIn >= @checkIn and[Booking].checkOut <= @checkOut)" +
            "or ([Booking].checkIn <= @checkOut and [Booking].checkOut >= @checkOut))", totalBeds, cInDate, cOutDate).Select(ro => new Room { ID = ro.ID, Level = ro.Level, BedCount = ro.BedCount,Price = ro.Price});

            ViewBag.Results = await searchRoom.ToListAsync();
            return View(roomSearch);
        }

        // GET: Bookings/CalcBookingStats
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CalcBookingStats()
        {
            // divide the Bookings into groups by postcode
            var customerGroups = _context.Customer.GroupBy(m => m.Postcode);
            var roomGroups = _context.Booking.GroupBy(m => m.RoomID);

            // for each group, get its count value and the number of purchases in this group
           var customerStats = customerGroups.Select(g => new BookingStatistic { PostCode = Convert.ToInt32(g.Key), CustomerCount = g.Count() });
           var bookingStats = roomGroups.Select(g => new BookingStatistic { RoomID = g.Key, BookingCount = g.Count() });

           ViewBag.TableA = await customerStats.ToListAsync();
           ViewBag.TableB = await bookingStats.ToListAsync();

            return View(await customerStats.ToListAsync());
        }

        // GET: Bookings/Details/5
        /*public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.TheRoom)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }*/

        // GET: Bookings/Create
        [Authorize(Roles = "Customers, Admin")]
        public IActionResult Create()
        {
            ViewData["RoomID"] = new SelectList(_context.Room, "ID", "ID");
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customers, Admin")]
        public async Task<IActionResult> Create(MakeBooking makeBooking)
        {
            if (ModelState.IsValid)
            {
                var rID = new SqliteParameter("rID", makeBooking.RoomID);
                var cInDate = new SqliteParameter("checkIn", makeBooking.CheckIn);
                var cOutDate = new SqliteParameter("checkOut", makeBooking.CheckOut);

                var searchRoom = _context.Room.FromSql("select * from [Room]"
                + "where [Room].ID = @rID and [Room].ID not in " +
                "(select [Room].ID from [Room] inner join [Booking] on [Room].ID = [Booking].RoomID " +
                "where ([Booking].checkIn <= @checkIn and [Booking].checkOut >= @checkIn)" +
                "or ([Booking].checkIn >= @checkIn and[Booking].checkOut <= @checkOut)" +
                "or ([Booking].checkIn <= @checkOut and [Booking].checkOut >= @checkOut))", rID, cInDate, cOutDate);

                ViewBag.Results = await searchRoom.ToListAsync();
                if (ViewBag.Results.Count != 0)
                {
                    if (User.IsInRole("Customers"))
                    {
                        var booking = new Booking()
                        {
                            RoomID = makeBooking.RoomID,
                            CustomerEmail = User.Identity.Name,
                            CheckIn = makeBooking.CheckIn,
                            CheckOut = makeBooking.CheckOut
                        };
                        var theRoom = await _context.Room.FindAsync(makeBooking.RoomID);
                        var lengthOfStay = (makeBooking.CheckOut - makeBooking.CheckIn).TotalDays;
                        
                        ViewData["MyBooking"] = booking;

                        if(lengthOfStay != 0)
                        {
                            booking.Cost = theRoom.Price * (decimal)lengthOfStay;
                        } else
                        {
                            booking.Cost = theRoom.Price;
                        }
                        

                        _context.Add(booking);
                        await _context.SaveChangesAsync();
                        return View(makeBooking);
                       // return RedirectToAction(nameof(Index));
                    } else
                    { 
                        var booking = new Booking()
                        {
                            RoomID = makeBooking.RoomID,
                            CustomerEmail = makeBooking.CustomerEmail,
                            CheckIn = makeBooking.CheckIn,
                            CheckOut = makeBooking.CheckOut
                        };
                        var theRoom = await _context.Room.FindAsync(makeBooking.RoomID);
                        var lengthOfStay = (makeBooking.CheckOut - makeBooking.CheckIn).TotalDays;

                        if (lengthOfStay != 0)
                        {
                            booking.Cost = theRoom.Price * (decimal)lengthOfStay;
                        }
                        else
                        {
                            booking.Cost = theRoom.Price;
                        }

                        _context.Add(booking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(BookingManagement));
                    }
                }

                ViewData["RoomID"] = new SelectList(_context.Room, "ID", "ID", makeBooking.RoomID);
                ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", makeBooking.CustomerEmail);
                return View(makeBooking);

            } 
            return View(makeBooking);
        }
        // GET: Bookings/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.ID == id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["RoomID"] = new SelectList(_context.Room, "ID", "ID", booking.RoomID);
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,RoomID,CustomerEmail,CheckIn,CheckOut,Cost")] Booking booking)
        {
            if (id != booking.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var theRoom = await _context.Room.FindAsync(booking.RoomID);
                    var lengthOfStay = (booking.CheckOut - booking.CheckIn).TotalDays;
                    booking.Cost = theRoom.Price * (decimal)lengthOfStay;
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(BookingManagement));
            }
            ViewData["RoomID"] = new SelectList(_context.Room, "ID", "ID", booking.RoomID);
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.TheRoom).Include(b => b.TheCustomer)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["CustomerEmail"] = new SelectList(_context.Customer, "Email", "Email", booking.CustomerEmail);
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.ID == id);
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BookingManagement));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.ID == id);
        }
    }
}
