using System.Collections.Concurrent;
using Linger.Email;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Moq;

namespace Linger.Email.UnitTests;

public class EmailTests
{
    private static EmailConfig CreateConfig() => new()
    {
        Host = "smtp.example.com",
        Port = 587,
        From = new EmailAddress { Address = "sender@example.com" }
    };

    private static Mock<ISmtpClient> CreateSmtpClientMock()
    {
        var mock = new Mock<ISmtpClient>();
        mock.SetupGet(client => client.IsConnected).Returns(true);
        mock.Setup(client => client.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<SecureSocketOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(client => client.AuthenticateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Returns((MimeMessage message, CancellationToken _, ITransferProgress _) =>
            {
                mock.Raise(client => client.MessageSent += null, new MessageSentEventArgs(message, $"RESP-{message.Subject}"));
                return Task.FromResult($"RESP-{message.Subject}");
            });
        mock.Setup(client => client.DisconnectAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static EmailMessage CreateMessage(string subject) => new()
    {
        To = [new EmailAddress { Address = "recipient@example.com" }],
        Subject = subject,
        Body = "Body"
    };

    private sealed class TestableEmail(EmailConfig config, Func<Mock<ISmtpClient>> clientFactory) : Email(config)
    {
        protected override ISmtpClient CreateClient() => clientFactory().Object;
    }

    [Fact]
    public void Email_DoesNotExposeUnneededDisposalContract()
    {
        Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(Email)));
        Assert.False(typeof(IAsyncDisposable).IsAssignableFrom(typeof(IEmail)));
    }

    [Fact]
    public async Task SendAsync_CompletedCallbackReceivesServerResponse()
    {
        var email = new TestableEmail(CreateConfig(), CreateSmtpClientMock);
        string? callbackResponse = null;

        await email.SendAsync(
            CreateMessage("Subject-1"),
            response => callbackResponse = response,
            TestContext.Current.CancellationToken);

        Assert.Equal("RESP-Subject-1", callbackResponse);
    }

    [Fact]
    public async Task SendAsync_ConcurrentSends_CreateClientPerSend()
    {
        const int sendCount = 20;
        var clientCount = 0;
        var email = new TestableEmail(CreateConfig(), () =>
        {
            Interlocked.Increment(ref clientCount);
            return CreateSmtpClientMock();
        });

        var tasks = Enumerable.Range(0, sendCount)
            .Select(i => email.SendAsync(CreateMessage($"Subject-{i}"), cancellationToken: TestContext.Current.CancellationToken));

        await Task.WhenAll(tasks);

        Assert.Equal(sendCount, clientCount);
    }

    [Fact]
    public async Task SendAsync_ConcurrentSends_RouteCallbacksToMatchingMessage()
    {
        const int sendCount = 20;
        var email = new TestableEmail(CreateConfig(), CreateSmtpClientMock);
        var responses = new ConcurrentDictionary<string, string>();

        var tasks = Enumerable.Range(0, sendCount)
            .Select(i => email.SendAsync(
                CreateMessage($"Subject-{i}"),
                response => responses[$"Subject-{i}"] = response,
                TestContext.Current.CancellationToken));

        await Task.WhenAll(tasks);

        Assert.Equal(sendCount, responses.Count);
        for (var i = 0; i < sendCount; i++)
        {
            Assert.Equal($"RESP-Subject-{i}", responses[$"Subject-{i}"]);
        }
    }

    [Fact]
    public async Task SendAsync_PathAttachment_DisposesOwnedStreamAfterSending()
    {
        var path = Path.Combine(Path.GetTempPath(), $"linger-email-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "attachment", TestContext.Current.CancellationToken);
        Stream? attachmentStream = null;
        var smtpClient = CreateSmtpClientMock();
        smtpClient.Setup(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Returns((MimeMessage message, CancellationToken _, ITransferProgress _) =>
            {
                var multipart = Assert.IsType<Multipart>(message.Body);
                var attachment = multipart.OfType<MimePart>().Single(part => part.IsAttachment);
                attachmentStream = attachment.Content!.Stream;
                return Task.FromResult("sent");
            });
        var email = new TestableEmail(CreateConfig(), () => smtpClient);

        try
        {
            var message = CreateMessage("Path attachment");
            message.AttachmentsPath = [path];

            await email.SendAsync(message, cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(attachmentStream);
            Assert.Throws<ObjectDisposedException>(() => attachmentStream!.ReadByte());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SendAsync_StreamAttachment_LeavesCallerOwnedStreamOpen()
    {
        using var attachmentStream = new MemoryStream("attachment"u8.ToArray());
        var email = new TestableEmail(CreateConfig(), CreateSmtpClientMock);
        var message = CreateMessage("Stream attachment");
        message.Attachments =
        [
            new AttachmentInfo
            {
                FileName = "attachment.txt",
                Stream = attachmentStream
            }
        ];

        await email.SendAsync(message, cancellationToken: TestContext.Current.CancellationToken);

        attachmentStream.Position = 0;
        Assert.Equal((int)'a', attachmentStream.ReadByte());
    }

    [Fact]
    public async Task SendAsync_Attachment_UsesConfiguredContentTransferEncoding()
    {
        MimeMessage? sentMessage = null;
        var smtpClient = CreateSmtpClientMock();
        smtpClient.Setup(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Returns((MimeMessage message, CancellationToken _, ITransferProgress _) =>
            {
                sentMessage = message;
                return Task.FromResult("sent");
            });
        var email = new TestableEmail(CreateConfig(), () => smtpClient);
        var message = CreateMessage("Encoded attachment");
        message.Attachments =
        [
            new AttachmentInfo
            {
                FileName = "attachment.txt",
                Data = "attachment"u8.ToArray(),
                ContentTransferEncoding = ContentEncoding.QuotedPrintable
            }
        ];

        await email.SendAsync(message, cancellationToken: TestContext.Current.CancellationToken);

        var multipart = Assert.IsType<Multipart>(sentMessage!.Body);
        var attachment = multipart.OfType<MimePart>().Single(part => part.IsAttachment);
        Assert.Equal(ContentEncoding.QuotedPrintable, attachment.ContentTransferEncoding);
    }
}
