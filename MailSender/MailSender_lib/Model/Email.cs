using MailSender_lib.Model.Base;

namespace MailSender_lib.Model
{
    public class Email : BaseEntity
    {
        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
