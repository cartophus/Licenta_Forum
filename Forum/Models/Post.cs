using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Add a title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Cannot create empty post")]
        public string Content { get; set; }

        public DateTime Created { get; set; }

        public string UserId { get; set; }
        [Required]
        public int ThreadId { get; set; }

        public virtual Thread Thread { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}