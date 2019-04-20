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

        public string UserId { get; set; }

        public int ThreadId { get; set; }

        //public virtual ApplicationUser User { get; set; }
        //public virtual Thread Thread { get; set; }

        [Required]
        public float Opinion { get; set; } //  0 1      
    }
}