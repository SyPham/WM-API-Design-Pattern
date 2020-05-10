using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Project
{
   public class TaskDetail
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string JobName { get; set; }
        public DateTime Deadline { get; set; }

        public int CreatedBy { get; set; }
        public int ProjectID { get; set; }
        public int ParentID { get; set; }
        public int OCID { get; set; }

        public string Remark { get; set; }

        public bool Status { get; set; }
        public int FromWhoID { get; set; }

        public int[] PIC { get; set; }
    }
}
