using MailSender_lib.Model.Base;

namespace MailSender_lib.Model
{
    public class Sender : HumanEntity
    {
        public Server Server { get; set; }
    }
}
