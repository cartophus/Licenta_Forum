using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class VoteThread
    {
        [Key]
        [Column(Order = 1)]
        public int VoteThreadId { get; set; }

        [Key]
        [Column(Order = 2)]
        public virtual ApplicationUser User { get; set; }

        [Key]
        [Column(Order = 3)]
        public virtual Thread Thread { get; set; }

        [Required]
        public string Opinion { get; set; } // "up" "down"

        
        
    }
}