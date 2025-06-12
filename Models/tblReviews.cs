using System;
using System.ComponentModel.DataAnnotations;

namespace WebGiaoDichViecLam.Models
{
    public class tblReviews
    {
        [Key]
        public int iReviewID {  get; set; }

        public int FK_iCompanyID { get; set; }

        public int FK_iAccountID { get; set; }

        public float fRate { get; set; }


        public string sReviewText { get; set; }

        public DateTime dReviewDate {  get; set; }
        
    }
}
