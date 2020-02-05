using System;
using System.Collections.Generic;
using MailSender_lib.Model.Base;

namespace MailSender_lib.Model
{
    public class ShedulerTask : BaseEntity
    {
        public DateTime Time { get; set; }

        public Recipient Recipient { get; set; }

        public Sender Sender { get; set; }

        public Email Email { get; set; }

        public string Status { get; set; }
    }
}
