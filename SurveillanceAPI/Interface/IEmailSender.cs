namespace SurveillanceAPI.Interface;

public interface IEmailSender
{
    Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct);
}