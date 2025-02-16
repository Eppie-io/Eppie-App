using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tuvi.Core.Entities;
using Tuvi.Core.Utils;

namespace Tuvi.App.ViewModels.Common
{
    using AttachmentsCollection = List<Attachment>;

    public class NewMessageData
    {
        public EmailAddress From { get; private set; }
        public string To { get; private set; }
        public string Copy { get; private set; }
        public string HiddenCopy { get; private set; }
        public string Subject { get; private set; }
        public string TextBody { get; private set; }
        public AttachmentsCollection Attachments { get; private set; }

        public NewMessageData(EmailAddress from, string to, string copy, string hiddenCopy, string subject, string textBody, AttachmentsCollection attachments = null)
        {
            From = from;
            To = to;
            Copy = copy;
            HiddenCopy = hiddenCopy;
            Subject = subject;
            TextBody = textBody;
            Attachments = attachments;
        }

        public virtual void OnDataLoadedAsync(Action<NewMessageData> action, TaskScheduler callerTaskScheduler, ITextUtils textUtils)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action(this);
        }

        protected static string CreateMessageBody(MessageInfo messageInfo, string bodyTemplate, ITextUtils textUtils)
        {
            if (textUtils == null)
            {
                throw new ArgumentNullException(nameof(textUtils));
            }

            var body = string.Empty;

            if (messageInfo != null)
            {
                body = messageInfo.HasTextBody ? messageInfo.MessageTextBody : textUtils.GetTextFromHtml(messageInfo.MessageHtmlBody);

                if (bodyTemplate != null)
                {
                    body = string.Format(CultureInfo.CurrentCulture, bodyTemplate, messageInfo.MessageSender, messageInfo.DateFullString, messageInfo.Email.Address, messageInfo.MessageSubject, body);
                }
            }

            return body;
        }

        protected static string CreateToAdress(MessageInfo messageInfo)
        {
            var replyTo = messageInfo.MessageData.ReplyTo.FirstOrDefault();
            return replyTo != null ? replyTo.Address : messageInfo.MessageData.From.FirstOrDefault()?.Address;
        }

        protected static string CreateReSubject(MessageInfo messageInfo)
        {
            const string replyPrefix = "Re: ";
            if (messageInfo?.MessageSubject != null && messageInfo.MessageSubject.StartsWith(replyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return messageInfo.MessageSubject;
            }
            return $"{replyPrefix}{messageInfo?.MessageSubject}";
        }
    }

    public class AsyncLoadMessageData : NewMessageData
    {
        public AsyncLoadMessageData(EmailAddress from, string to, string copy, string hiddenCopy, string subject, string textBody, string template, AttachmentsCollection attachments, Task<MessageInfo> activeMessageInfoLoadTask)
             : base(from, to, copy, hiddenCopy, subject, textBody, attachments)
        {
            messageInfoLoadTask = activeMessageInfoLoadTask;
            bodyTemplate = template;
        }

        public override void OnDataLoadedAsync(Action<NewMessageData> action, TaskScheduler callerTaskScheduler, ITextUtils textUtils)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (callerTaskScheduler == null)
            {
                throw new ArgumentNullException(nameof(callerTaskScheduler));
            }

            if (messageInfoLoadTask == null)
            {
                action(this);
            }
            else
            {
                messageInfoLoadTask.ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted)
                        {
                            action(null);
                        }
                        else
                        {
                            action(Create(task.Result, Attachments, bodyTemplate, textUtils));
                        }
                    },
                    callerTaskScheduler);
            }
        }

        protected virtual AsyncLoadMessageData Create(MessageInfo messageInfo, AttachmentsCollection attachments, string template, ITextUtils textUtils)
        {
            throw new NotImplementedException();
        }

        private readonly Task<MessageInfo> messageInfoLoadTask;
        private readonly string bodyTemplate;
    }

    public class ReplyMessageData : AsyncLoadMessageData
    {
        public ReplyMessageData(MessageInfo messageInfo, string template, ITextUtils textUtils, Task<MessageInfo> activeMessageInfoLoadTask = null)

             : base(messageInfo?.Email, CreateToAdress(messageInfo), string.Empty, string.Empty, CreateSubject(messageInfo), CreateMessageBody(messageInfo, template, textUtils), template, null, activeMessageInfoLoadTask)
        { }

        protected ReplyMessageData(MessageInfo messageInfo, string template, ITextUtils textUtils)
            : base(messageInfo?.Email, CreateToAdress(messageInfo), string.Empty, string.Empty, CreateSubject(messageInfo), CreateMessageBody(messageInfo, template, textUtils), template, null, null)
        { }

        protected override AsyncLoadMessageData Create(MessageInfo messageInfo, AttachmentsCollection attachments, string template, ITextUtils textUtils)
        {
            return new ReplyMessageData(messageInfo, template, textUtils);
        }

        private static string CreateSubject(MessageInfo messageInfo)
        {
            return CreateReSubject(messageInfo);
        }
    }

    public class ReplyAllMessageData : AsyncLoadMessageData
    {
        public ReplyAllMessageData(MessageInfo messageInfo, string template, ITextUtils textUtils, Task<MessageInfo> activeMessageInfoLoadTask = null)
             : base(messageInfo?.Email, CreateToAdresses(messageInfo), CreateCopyAdresses(messageInfo), messageInfo?.MessageHiddenCopy, CreateSubject(messageInfo), CreateMessageBody(messageInfo, template, textUtils), template, null, activeMessageInfoLoadTask)
        { }

        protected ReplyAllMessageData(MessageInfo messageInfo, string template, ITextUtils textUtils)
            : base(messageInfo?.Email, CreateToAdresses(messageInfo), CreateCopyAdresses(messageInfo), messageInfo?.MessageHiddenCopy, CreateSubject(messageInfo), CreateMessageBody(messageInfo, template, textUtils), template, null, null)
        { }

        protected override AsyncLoadMessageData Create(MessageInfo messageInfo, AttachmentsCollection attachments, string template, ITextUtils textUtils)
        {
            return new ReplyAllMessageData(messageInfo, template, textUtils);
        }

        private static string CreateToAdresses(MessageInfo messageInfo)
        {
            var toEmails = new List<string>
            {
                CreateToAdress(messageInfo)
            };
            toEmails.AddRange(messageInfo.MessageData.To.Where(email => !messageInfo.Email.HasSameAddress(email)).Select(email => email.Address));

            var to = string.Join(", ", toEmails.Distinct());

            return to;
        }

        private static string CreateCopyAdresses(MessageInfo messageInfo)
        {
            var ccEmails = new List<string>();
            ccEmails.AddRange(messageInfo.MessageData.Cc.Where(email => !messageInfo.Email.HasSameAddress(email)).Select(email => email.Address));

            var copy = string.Join(", ", ccEmails.Distinct());

            return copy;
        }

        private static string CreateSubject(MessageInfo messageInfo)
        {
            return CreateReSubject(messageInfo);
        }
    }

    public class ForwardMessageData : AsyncLoadMessageData
    {
        public ForwardMessageData(MessageInfo messageInfo, string template, ITextUtils textUtils, Task<MessageInfo> activeMessageInfoLoadTask = null)
             : base(messageInfo?.Email, string.Empty, string.Empty, string.Empty, CreateSubject(messageInfo), CreateMessageBody(messageInfo, template, textUtils), template, messageInfo?.Attachments, activeMessageInfoLoadTask)
        { }

        protected ForwardMessageData(MessageInfo messageInfo, AttachmentsCollection attachments, string template, ITextUtils textUtils)
             : base(messageInfo?.Email, string.Empty, string.Empty, string.Empty, CreateSubject(messageInfo), CreateMessageBody(messageInfo, template, textUtils), template, attachments, null)
        { }

        protected override AsyncLoadMessageData Create(MessageInfo messageInfo, AttachmentsCollection attachments, string template, ITextUtils textUtils)
        {
            return new ForwardMessageData(messageInfo, attachments, template, textUtils);
        }

        private static string CreateSubject(MessageInfo messageInfo)
        {
            return $"FW: {messageInfo?.MessageSubject}";
        }
    }

    public class DraftMessageData : AsyncLoadMessageData
    {
        public MessageInfo MessageInfo { get; private set; }

        public DraftMessageData(MessageInfo messageInfo, ITextUtils textUtils, Task<MessageInfo> activeMessageInfoLoadTask = null)
             : base(messageInfo?.Email, messageInfo?.MessageReceiver, messageInfo?.MessageCopy, messageInfo?.MessageHiddenCopy, messageInfo?.MessageSubject, CreateMessageBody(messageInfo, null, textUtils), null, messageInfo?.Attachments, activeMessageInfoLoadTask)
        {
            MessageInfo = messageInfo;
        }

        protected DraftMessageData(MessageInfo messageInfo, ITextUtils textUtils, AttachmentsCollection attachments)
            : base(messageInfo?.Email, messageInfo?.MessageReceiver, messageInfo?.MessageCopy, messageInfo?.MessageHiddenCopy, messageInfo?.MessageSubject, CreateMessageBody(messageInfo, null, textUtils), null, attachments, null)
        {
            MessageInfo = messageInfo;
        }

        protected override AsyncLoadMessageData Create(MessageInfo messageInfo, AttachmentsCollection attachments, string template, ITextUtils textUtils)
        {
            return new DraftMessageData(messageInfo, textUtils, attachments);
        }
    }

    public class SharePublicKeyMessageData : NewMessageData
    {
        public SharePublicKeyMessageData(string userIdentity, byte[] keyFileData, string keyFileName, string messageSubject)
            : base(new EmailAddress(userIdentity),
                   String.Empty,
                   String.Empty,
                   String.Empty,
                   messageSubject,
                   String.Empty,
                   new AttachmentsCollection
                   {
                       new Attachment
                       {
                           FileName = keyFileName,
                           Data = keyFileData
                       }
                   })
        { }
    }

    public class SelectedAccountNewMessageData : NewMessageData
    {
        public SelectedAccountNewMessageData(EmailAddress email)
            : base(email,
                   String.Empty,
                   String.Empty,
                   String.Empty,
                   String.Empty,
                   String.Empty)
        { }
    }

    public class SelectedContactNewMessageData : NewMessageData
    {
        public SelectedContactNewMessageData(EmailAddress email, EmailAddress contactEmail)
            : base(email,
                   contactEmail != null ? contactEmail.Address : String.Empty,
                   String.Empty,
                   String.Empty,
                   String.Empty,
                   String.Empty)
        { }
    }

    public class ErrorReportNewMessageData : NewMessageData
    {
        public ErrorReportNewMessageData(string support, string title, string message)
            : base(new EmailAddress(""),
                   support,
                   String.Empty,
                   String.Empty,
                   title,
                   message)
        { }
    }

}
