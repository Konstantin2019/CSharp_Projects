using MailSender_lib.Model;
using MailSender_lib.Services.Abstract;

namespace MailSender_lib.Services.InMemory
{
    public class InMemoryServerProvider : InMemoryDataProvider<Server>
    {
        public InMemoryServerProvider(string filename)
        {
            path = filename;
            ReadData();
        }

        public override void Edit(int id, Server item)
        {
            var server = GetById(id);
            if (server is null) return;
            server.Name = item.Name;
            server.Host = item.Host;
            server.Ssl = item.Ssl;
            server.UserName = item.UserName;
            server.Password = item.Password;
            server.Port = item.Port;
            server.Description = item.Description;
        }
    }
}
