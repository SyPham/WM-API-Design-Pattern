using System;
using System.Collections.Generic;
using System.Text;

namespace WM.Application.ViewModel.Tutorial
{
    public class TutorialViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Path { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }
    }
}
