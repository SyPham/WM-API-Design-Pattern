using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WM.Application.ViewModel.OC
{
    public class TreeViewOC
    {
        public TreeViewOC()
        {
            this.children = new List<TreeViewOC>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int ParentID { get; set; }

        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public List<TreeViewOC> children { get; set; }
    }
}
