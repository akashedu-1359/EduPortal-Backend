using EduPortal.Domain.Entities;
using FluentAssertions;

namespace EduPortal.Tests.Domain;

public class CertificateTests
{
    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();

        var cert = Certificate.Create(userId, attemptId, "certificates/cert-123.pdf");

        cert.UserId.Should().Be(userId);
        cert.ExamAttemptId.Should().Be(attemptId);
        cert.StorageKey.Should().Be("certificates/cert-123.pdf");
        cert.IssuedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var cert1 = Certificate.Create(Guid.NewGuid(), Guid.NewGuid(), "key1");
        var cert2 = Certificate.Create(Guid.NewGuid(), Guid.NewGuid(), "key2");

        cert1.Id.Should().NotBe(cert2.Id);
        cert1.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_SetsIssuedAtToCurrentTime()
    {
        var before = DateTime.UtcNow;
        var cert = Certificate.Create(Guid.NewGuid(), Guid.NewGuid(), "key");
        var after = DateTime.UtcNow;

        cert.IssuedAt.Should().BeOnOrAfter(before);
        cert.IssuedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Create_LinksToBothUserAndAttempt()
    {
        var userId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();

        var cert = Certificate.Create(userId, attemptId, "cert.pdf");

        cert.UserId.Should().Be(userId);
        cert.ExamAttemptId.Should().Be(attemptId);
    }

    [Fact]
    public void Create_WithDifferentStorageKeys_SetsCorrectKey()
    {
        var cert1 = Certificate.Create(Guid.NewGuid(), Guid.NewGuid(), "certificates/user1/cert.pdf");
        var cert2 = Certificate.Create(Guid.NewGuid(), Guid.NewGuid(), "certs/exam-456.pdf");

        cert1.StorageKey.Should().Be("certificates/user1/cert.pdf");
        cert2.StorageKey.Should().Be("certs/exam-456.pdf");
    }
}
