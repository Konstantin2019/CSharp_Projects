using MailSender_lib.Model.Base;

namespace MailSender_lib.Model
{
    public class Sender : HumanEntity
    {
        public string Password { get; set; }

        public Server Server { get; set; }
    }
}
