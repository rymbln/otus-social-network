using System;
using System.ComponentModel.DataAnnotations;

namespace OtusSocialNetwork.DataClasses.Requests
{
	public class UpdatePostReq
	{
		public UpdatePostReq()
		{
		}
        [Required]
        public string Id { get; set; }
        [Required]
        public string Text { get; set; }
    }
}

