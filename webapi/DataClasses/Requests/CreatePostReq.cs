using System;
using System.ComponentModel.DataAnnotations;

namespace OtusSocialNetwork.DataClasses.Requests
{
	public class CreatePostReq
	{
		public CreatePostReq()
		{
		}

        [Required]
        public string Text { get; set; }
    }
}

