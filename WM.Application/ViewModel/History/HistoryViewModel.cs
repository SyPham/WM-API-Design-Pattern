using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.History
{
    public class HistoryViewModel
    {
        public int TaskID { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string TaskName { get; set; }
    }
}
