using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wexflow.Tasks.MailsSender
{
    public class Mail
    {
        public string From {get; private set;}
        public string[] To { get; private set; }
        public string[] Cc { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }

        public Mail(string from, string[] to, string[] cc, string subject, string body)
        {
            this.From = from;
            this.To = to;
            this.Cc = cc;
            this.Subject = subject;
            this.Body = body;
        }

        public void Send(string host, int port, bool enableSsl, string user, string password)
        {
            SmtpClient smtp = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password)
            };

            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(this.From);
                foreach (string to in this.To) msg.To.Add(new MailAddress(to));
                foreach (string cc in this.Cc) msg.CC.Add(new MailAddress(cc));
                msg.Subject = this.Subject;
                msg.Body = this.Body;

                smtp.Send(msg);
            }
        }

        public static Mail Parse(XElement xe)
        {
            string from = xe.XPathSelectElement("From").Value;
            string[] to = xe.XPathSelectElement("To").Value.Split(',');
            string[] cc = xe.XPathSelectElement("Cc").Value.Split(',');
            string subject = xe.XPathSelectElement("Subject").Value;
            string body = xe.XPathSelectElement("Body").Value;

            return new Mail(from, to, cc, subject, body);
        }
    }
}
