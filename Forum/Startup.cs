using Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using ShieldUI.AspNetCore.Mvc;

[assembly: OwinStartupAttribute(typeof(Forum.Startup))]
namespace Forum
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            createAdminUserAndApplicationRoles();

            //app.UseShieldUI();
        }

        public void ConfigureServices(IServiceCollection services)
        {

            //services.AddShieldUI();
        }

        private void createAdminUserAndApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new
           RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new
           UserStore<ApplicationUser>(context));
            // Se adauga rolurile aplicatiei
            if (!roleManager.RoleExists("Administrator"))
            {
                // Se adauga rolul de administrator
                var role = new IdentityRole();
                role.Name = "Administrator";
                roleManager.Create(role);
                // se adauga utilizatorul administrator
                var user = new ApplicationUser();
                user.UserName = "vled.nastasa@gmail.com";
                user.Email = "vled.nastasa@gmail.com";
                user.UserPhoto = "/Content/Images/UserPhotos/0.jpg";
                var adminCreated = UserManager.Create(user, "Katonshido98");
                if (adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Administrator");
                }
            }
            if (!roleManager.RoleExists("Editor"))
            {
                var role = new IdentityRole();
                role.Name = "Editor";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole();
                role.Name = "User";
                roleManager.Create(role);
            }
        }
    }
}
