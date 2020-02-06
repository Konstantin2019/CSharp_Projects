using MailSender_lib.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace MailSender_lib.Services
{
    public class MailSenderService
    {
        public async Task<Response> SendAsync(Sender sender, Recipient recipient, Email email)
        {
            var server = sender.Server;

            using (var client = new SmtpClient(server.Host, server.Port))
            {
                client.EnableSsl = server.Ssl;
                client.Credentials = new NetworkCredential(server.UserName, server.Password);

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(sender.Address, sender.Name);
                    message.To.Add(new MailAddress(recipient.Address, recipient.Name));
                    message.Subject = email.Subject;
                    message.Body = email.Body;

                    try
                    {
                        await client.SendMailAsync(message);
                        return new Response { Success = true, Error = null };
                    }
                    catch (Exception error)
                    {
                        return new Response { Success = false, Error = error.Message };
                    }
                }
            }
        }

        public IEnumerable<Response> SendAsync(Sender sender, IEnumerable<Recipient> recipients, Email email)
        {

            var responses = new ConcurrentQueue<Response>();

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };
            Parallel.ForEach(recipients, options, recipient =>
            {
                Task.Run(async () =>
                    {
                        var response = await SendAsync(sender, recipient, email);
                        responses.Enqueue(response);
                    }).ConfigureAwait(false);
            });

            return responses.ToArray();
        }
    }
}
