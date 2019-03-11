/*using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class User
    {
        [Key]
        public string UserID { get; set; }

        [Required(ErrorMessage = "Username field required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password field required")]
        public string UserPassword { get; set; }

        [Required(ErrorMessage = "First name field required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name field required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-mail field required")]
        [EmailAddress(ErrorMessage = "Invalid e-mail address")]
        public string Email { get; set; }

        public DateTime Created { get; set; }

        /// FK restrictions
        public virtual ICollection<Post> Posts { get; set; }
    }
}*/