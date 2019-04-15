using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Models
{
    public class Thread
    {
        [Key]
        public int ThreadId { get; set; }
        [Required(ErrorMessage = "Title field required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content field required")]
        public string Content { get; set; }

        public DateTime Created { get; set; }

        //geolocation
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /// FK
        [Required]
        public string UserId { get; set; }
       
        [Required]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        /// FK restrictions
        public virtual Category Category { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<VoteThread> VoteThreads { get; set; }
    }
}