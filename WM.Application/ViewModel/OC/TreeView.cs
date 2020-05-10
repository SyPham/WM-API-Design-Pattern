using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WM.Application.ViewModel.OC
{
    public class TreeView
    {
        public TreeView()
        {
            this.children = new List<TreeView>();
        }

        public int key { get; set; }
        public string title { get; set; }
        public string code { get; set; }
        public int levelnumber { get; set; }
        public int parentid { get; set; }
        public bool state { get; set; }

        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public List<TreeView> children { get; set; }
    }
}
