using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WM.Data.Entities
{
   public class OCUser
    {
        [Key, Column(Order = 0)]
        public int UserID { get; set; }
        [Key, Column(Order = 1)]
        public int OCID { get; set; }
        public bool Status { get; set; }
        public virtual OC OC { get; set; }
        public virtual User User { get; set; }
    }
}
