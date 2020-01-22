using MailSender_lib.Model.Base;

namespace MailSender_lib.Model
{
    public class Server : NamedEntity
    {
        public int Port { get; set; } = 25;

        public string Host { get; set; }

        public bool Ssl { get; set; } = true;

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Description { get; set; }
    }
}
