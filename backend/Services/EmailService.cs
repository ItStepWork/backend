using MimeKit;
using MailKit.Security;

namespace backend.Services
{
    public static class EmailService
    {
        public static async Task SendEmailAsync(string email, string subject, string name, string lastname, string joined, string password, string[] descriptions)
        {
            
            using var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("", ConfigurationManager.AppSetting["SMPT_NAME"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = "<div" +
                "style=\"background-color: #eff2f6;" +
                "height: 600px; width: 30%; margin: 0 auto; color: black" +
                "border-radius: 10px; display: flex; padding-left: 20px;" +
                "font-family: Verdana, Geneva, Tahoma, sans-serif;" +
                "flex-direction: column;\">" +
                $"<h3>{descriptions[0]} <span style=\"color: #0f6fec;\">CONNECTIONS</span></h3>" +
                "<div>" +
                $"<p>{descriptions[1]}</p>" +
                "<hr style=\"margin-right: 20px;\">" +
                $"<p>{descriptions[2]}</p>" +
                "<ul >" +
                $"<li style=\"padding-bottom: 10px;\">Имя: {name}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Фамилия: {lastname}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Email: {email}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Присоединились к нам: {joined}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Пароль: {password}</li>" +
                "</ul>" +
                "</div>" +
                "</div>"

            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(ConfigurationManager.AppSetting["SMPT_SERVER"], 465, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(ConfigurationManager.AppSetting["SMPT_NAME"], ConfigurationManager.AppSetting["SMPT_PASSWORD"]);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
