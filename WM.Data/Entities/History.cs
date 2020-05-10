
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
            this.ModifyDateTime = DateTime.Now.ToString("d MMM, yyyy hh:mm:ss tt");
        }

        public string TaskCode { get; set; }
        [ForeignKey("TaskID")]
        public int TaskID { get; set; }
        public bool Status { get; set; }
        public string Deadline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifyDateTime { get; set; }
        public virtual Task Task { get; set; }
    }
}
