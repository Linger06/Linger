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
}
