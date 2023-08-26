using MimeKit;
using MailKit.Security;

namespace backend.Services
{
    public static class EmailService
    {
        public static async Task SendEmailAsync(string email, string subject, string name, string lastname, string joined )
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
                "<h3>Добро пожаловать в семью <span style=\"color: #0f6fec;\">CONNECTIONS</span></h3>" +
                "<div>" +
                "<p>Спасибо за регистрацию на в нашей социальной сети</p>" +
                "<hr style=\"margin-right: 20px;\">" +
                "<p>Ваши данные при регистрации:</p>" +
                "<ul >" +
                $"<li style=\"padding-bottom: 10px;\">Имя: {name}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Фамилия: {lastname}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Email: {email}</li>" +
                $"<li style=\"padding-bottom: 10px;\">Присоединились к нам: {joined}</li>" +
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
            
            /*
            MailMessage mail = new MailMessage("***@gmail.com", email);
            mail.Body = message;
            mail.Subject = subject;
            SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 465);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("***@gmail.com", "mypass");
            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Exception ex2 = ex;
                string errorMessage = string.Empty;
                while (ex2 != null)
                {
                    errorMessage += ex2.ToString();
                    ex2 = ex2.InnerException;
                }
            }
            */
        }
    }
}
