using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Forum.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim("UserPhoto", this.UserPhoto.ToString()));

            return userIdentity;
        }

        public string UserPhoto { get; set; }

        public IEnumerable<SelectListItem> AllRoles { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Thread> Threads { get; set; }
        public virtual ICollection<VoteThread> VoteThreads { get; set; }
        public virtual ICollection<VotePost> VotePosts { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<VoteThread> VoteThreads { get; set; }
        public DbSet<VotePost> VotePosts { get; set; }
 
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}