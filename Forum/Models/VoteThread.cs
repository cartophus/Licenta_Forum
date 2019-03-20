using Microsoft.ML.Data;
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
        public int VoteThreadId { get; set; }

        [Key]
        public virtual ApplicationUser User { get; set; }

        [Key]
        public virtual Thread Thread { get; set; }

        [Required]
        public float Opinion { get; set; } //  0 1      
    }
}