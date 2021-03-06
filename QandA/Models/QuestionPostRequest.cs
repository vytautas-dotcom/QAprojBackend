using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QandA.Models
{
    public class QuestionPostRequest
    {
        [Required(ErrorMessage = "Title is requied")]
        [StringLength(100)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }
    }
}
