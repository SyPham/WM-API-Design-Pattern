using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class UploadImage : DomainEntity<int>
    {
        [ForeignKey("CommentID")]
        public int CommentID { get; set; }
        [ForeignKey("ChatID")]
        public int ChatID { get; set; }
        public string Image { get; set; }
        public virtual Chat Chat { get; set; }
        public virtual Comment Comment { get; set; }
    }
}
