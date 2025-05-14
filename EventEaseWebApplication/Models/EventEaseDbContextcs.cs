using Microsoft.EntityFrameworkCore;
using EventEaseWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventEaseWebApplication.Models
{
    public class EventEaseDbContextcs : DbContext
    {
        public EventEaseDbContextcs(DbContextOptions<EventEaseDbContextcs> options) : base(options)
        {
            
        }
        public DbSet<Event> Event { get; set; }
        public DbSet<Venue> Venue { get; set; }
        public DbSet<Booking> Booking { get; set; }
    }
}
