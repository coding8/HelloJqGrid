using System.Data.Entity;

namespace HelloJqGrid.Models
{
    public class MyContext:DbContext
    {
        public MyContext():base("name=DefaultConnection"){}

        public DbSet<Guestbook> Guestbooks { get; set; }
        public DbSet<Member> Members { get; set; }
    }
}