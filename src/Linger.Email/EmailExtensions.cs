namespace Linger.Email;

public static class EmailExtensions
{
    /// <summary>
    ///     Splits the recipients. 拆分以','或';'分隔的收件人
    /// </summary>
    /// <param name="mailboxes">The mailboxes.</param>
    public static List<EmailAddress> SplitRecipients(this string? mailboxes)
    {
        ArgumentNullException.ThrowIfNull(mailboxes);

        var splitMailboxes = mailboxes.Split([",", ";"], StringSplitOptions.RemoveEmptyEntries);

        var list = new List<EmailAddress>();

        foreach (var box in splitMailboxes)
        {
            if (box == null)
            {
                throw new NullReferenceException(nameof(box));
            }

            var address = new EmailAddress { Address = box };
            list.Add(address);
        }

        return list;
    }
}