using MailSender_lib.Model;
using MailSender_lib.Services.Abstract;

namespace MailSender_lib.Services.InMemory
{
    public class InMemoryRecipientProvider : InMemoryDataProvider<Recipient>
    {
        public InMemoryRecipientProvider(string filename)
        {
            path = filename;
            Init();
        }

        public override void Edit(int id, Recipient item)
        {
            var recipient = GetById(id);
            if (recipient is null) return;
            recipient.Name = item.Name;
            recipient.Address = item.Address;
        }
    }
}
