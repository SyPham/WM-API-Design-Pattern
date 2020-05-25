
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
    public class History : DomainEntity<int>
    {
        public History()
        {
            CreatedDate = DateTime.Now;
            this.ModifyDateTime = DateTime.Now;
        }

        public string TaskCode { get; set; }
        public int TaskID { get; set; }
        public int UserID { get; set; }
        public bool Status { get; set; }
        public string Deadline { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public virtual Task Task { get; set; }
    }
}
