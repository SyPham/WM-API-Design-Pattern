using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using WM.Infrastructure.SharedKernel;

namespace WM.Data.Entities
{
   public class UploadImage : DomainEntity<int>
    {
        public int CommentID { get; set; }
        public int ChatID { get; set; }
        public string Image { get; set; }
    }
}
