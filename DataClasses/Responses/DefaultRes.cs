using System;
namespace OtusSocialNetwork.DataClasses.Responses
{
	public class DefaultRes
	{
		public DefaultRes(string res)
		{
			Result = res;
		}
        public string Result { get; set; }
    }
}

