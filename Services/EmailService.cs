using System.Net;
using System.Net.Mail;

namespace Blog.Services;

public class EmailService
{
    public bool Send(string toName, string toEmail, string subject, string body, string fromName="Equipe Balta io", string fromEmail = "email@balta.io")
    {
        var stmpClient = new SmtpClient(Configuration.Stmp.Host, Configuration.Stmp.Port);      

        stmpClient.Credentials  = new NetworkCredential(Configuration.Stmp.Username, Configuration.Stmp.Password);
        stmpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        stmpClient.EnableSsl = true;

        var email = new MailMessage();
        email.From = new MailAddress(fromEmail, fromName);
        email.To.Add(new MailAddress(toEmail, toName)); //pode adicionar quantos emails de destinatario quiser
        email.Subject =  subject;
        email.Body = body;
        email.IsBodyHtml = true; // permite que os comandos html sejam interpretados

        try
        {
            stmpClient.Send(email);
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    }
}