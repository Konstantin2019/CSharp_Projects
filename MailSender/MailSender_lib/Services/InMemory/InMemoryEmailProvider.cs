using MailSender_lib.Model;
using MailSender_lib.Services.Abstract;

namespace MailSender_lib.Services.InMemory
{
    public class InMemoryEmailProvider : InMemoryDataProvider<Email>
    {
        public InMemoryEmailProvider(string filename)
        {
            path = filename;
            ReadData();
        }

        public override void Edit(int id, Email item)
        {
            var email = GetById(id);
            if (email is null) return;
            email.Subject = item.Subject;
            email.Body = item.Body;
        }
    }
}
