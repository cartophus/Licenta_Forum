using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class VoteThreadPrediction
    {
        [ColumnName("Score")]
        public float OpinionPrediction;
    }
}