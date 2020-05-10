using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Project
{
    public class ProjectViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public int Room { get; set; }
        public bool Status { get; set; }
        public List<int> Members { get; set; }
        public List<int> Manager { get; set; }
    }
}
