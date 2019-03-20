using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class VoteThreadMask
    {
        [LoadColumn(0)]
        public float VoteThreadId;

        [LoadColumn(1)]
        public string UserId;

        [LoadColumn(2)]
        public float ThreadId;

        [LoadColumn(3)]
        public float Opinion { get; set; } //  0 1  
    }
}