using MailSender_lib.Model;
using MailSender_lib.Services.Abstract;

namespace MailSender_lib.Services.InMemory
{
    public class InMemorySenderProvider : InMemoryDataProvider<Sender>
    {
        public InMemorySenderProvider(string filename)
        {
            path = filename;
            ReadData();
        }

        public override void Edit(int id, Sender item)
        {
            var sender = GetById(id);
            if (sender is null) return;
            sender.Name = item.Name;
            sender.Address = item.Address;
        }
    }
}
