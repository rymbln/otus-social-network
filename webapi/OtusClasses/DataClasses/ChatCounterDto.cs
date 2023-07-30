using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusClasses.DataClasses;

public class ChatCounterDto
{
    public ChatCounterDto()
    {
    }

    public ChatCounterDto(string from, string to, int unreads)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
        Unreads = unreads;
    }

    public string From { get; set; }
    public string To { get; set; }
    public int Unreads { get; set; }
}
