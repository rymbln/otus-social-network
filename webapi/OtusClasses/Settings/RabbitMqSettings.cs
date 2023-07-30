namespace OtusClasses.Settings;

public class RabbitMqSettings
{
    public string Uri { get; set; } = null!;
    public string SagaUri { get; set; } = null!;
    public string SagaQueue { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Consumer { get; set; } = null!;
}
