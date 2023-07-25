namespace OtusSocialNetwork.DataClasses.Dtos
{
    public class DialogMessageDTO
    {
        public DialogMessageDTO(string from, string to, string text)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public DialogMessageDTO(string from, string to, string text, DateTime timeStamp) : this(from, to, text)
        {
            TimeStamp = timeStamp;
        }

        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }

        public DateTime TimeStamp { get;set; }
    }

    public class DialogMessageForm
    {
        public string Text { get; set; }
    }
}
