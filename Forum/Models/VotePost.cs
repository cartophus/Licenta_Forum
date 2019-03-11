using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class VotePost
    {
        [Key]
        [Column(Order = 1)]
        public int VotePostId { get; set; }

        [Key]
        [Column(Order = 2)]
        public virtual ApplicationUser User { get; set; }
        //public string UserId { get; set; }

        [Key]
        [Column(Order = 3)]
        public virtual Post Post { get; set; }
        //public int PostId { get; set; }

        [Required]
        public string Opinion { get; set; } // "up" "down"

        
        
    }
}