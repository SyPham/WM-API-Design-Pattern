
using Data.ViewModel.Tutorial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WM.Application.ViewModel.History;
using WM.Application.ViewModel.Tutorial;
using WM.Data.Entities;
using WM.Data.Enums;

namespace WM.Application.ViewModel.Project
{
    public class TreeViewTask
    {
        public TreeViewTask()
        {
            this.children = new List<TreeViewTask>();
        }

        public int ID { get; set; }
        public string Follow { get; set; }
        public string Priority { get; set; }
        public string PriorityID { get; set; }
        public string TaskCode { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string PIC { get; set; }
        public bool VideoStatus { get; set; }
        public TreeViewTutorial Tutorial { get; set; } = new TreeViewTutorial();
        public string VideoLink { get; set; } = string.Empty;
        public int Level { get; set; }
        public int ParentID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DeputyName { get; set; } = string.Empty;
        public JobType JobTypeID { get; set; }
        public int FromWhoID { get; set; }
        public PeriodType periodType { get; set; }
        public string From { get; set; }
        public int CreatedBy { get; set; }
        public string LastComment { get; set; }
        public int ProjectID { get; set; } = 0;
        public BeAssigned User { get; set; }
        public List<BeAssigned> BeAssigneds { get; set; }
        public List<BeAssigned> DeputiesList { get; set; }
        public List<int> Deputies { get; set; }
        public List<int> PICs { get; set; }
        public BeAssigned FromWho { get; set; }
        public FromWhere FromWhere { get; set; }
        public ProjectViewModel Project { get; set; } = new ProjectViewModel();
        public List<HistoryViewModel> Histories { get; set; } = new List<HistoryViewModel>();
        public string state { get; set; }
        public bool FinishTask { get; set; }
        public DateTime DueDate { get; set; }
        public string ModifyDateTime { get; set; }
        public List<Follow> Follows { get; set; } = new List<Data.Entities.Follow>();
        public List<TreeViewTask> RelatedTasks { get; set; } = new List<TreeViewTask>();
        public bool BeAssigned { get; set; }
        public bool HasChildren
        {
            get { return children.Any(); }
        }

        public List<TreeViewTask> children { get; set; }



        public static List<TreeViewTask> Flatten(TreeViewTask root)
        {

            var flattened = new List<TreeViewTask> { root };

            var children = root.children;

            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(Flatten(child));
                }
            }

            return flattened;
        }
    }

    public class BeAssigned
    {
        public int ID { get; set; }
        public string Username { get; set; }
    }
    public class FromWhere
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    public class RelatedTask
    {
        public int ID { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
