using MailSender_lib.Model;
using MailSender_lib.Services.Abstract;

namespace MailSender_lib.Services.InMemory
{
    public class InMemoryShedulerProvider : InMemoryDataProvider<ShedulerTask>
    {
        public InMemoryShedulerProvider(string filename)
        {
            path = filename;
            Init();
        }

        public override void Edit(int id, ShedulerTask item)
        {
            var shedulerTask = GetById(id);
            if (shedulerTask is null) return;
            shedulerTask.Time = item.Time;
            shedulerTask.Sender = item.Sender;
            shedulerTask.Recipient = item.Recipient;
            shedulerTask.Recipients = item.Recipients;
            shedulerTask.Email = item.Email;
        }
    }
}
