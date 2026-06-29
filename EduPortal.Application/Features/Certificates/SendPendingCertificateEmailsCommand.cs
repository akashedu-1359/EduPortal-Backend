using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduPortal.Application.Features.Certificates;

public record SendPendingCertificateEmailsCommand : IRequest<SendPendingCertificateEmailsResult>;

public record SendPendingCertificateEmailsResult(int Sent, int Skipped, int Failed);

public class SendPendingCertificateEmailsCommandHandler
    : IRequestHandler<SendPendingCertificateEmailsCommand, SendPendingCertificateEmailsResult>
{
    private readonly ICmsRepository _cms;
    private readonly IExamRepository _exams;
    private readonly IMediator _mediator;
    private readonly ILogger<SendPendingCertificateEmailsCommandHandler> _logger;

    public SendPendingCertificateEmailsCommandHandler(
        ICmsRepository cms,
        IExamRepository exams,
        IMediator mediator,
        ILogger<SendPendingCertificateEmailsCommandHandler> logger)
    {
        _cms = cms;
        _exams = exams;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<SendPendingCertificateEmailsResult> Handle(
        SendPendingCertificateEmailsCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _cms.IsFeatureEnabledAsync(FeatureFlagKeys.EnableCertificates, cancellationToken))
        {
            return new SendPendingCertificateEmailsResult(0, 0, 0);
        }

        var pending = await _exams.GetCertificatesPendingEmailAsync(cancellationToken);
        var sent = 0;
        var failed = 0;

        foreach (var certificate in pending)
        {
            var success = await _mediator.Send(new SendCertificateEmailCommand(certificate.Id), cancellationToken);
            if (success) sent++;
            else failed++;
        }

        _logger.LogInformation(
            "Pending certificate emails processed: {Sent} sent, {Failed} failed, {Total} total.",
            sent,
            failed,
            pending.Count);

        return new SendPendingCertificateEmailsResult(sent, 0, failed);
    }
}
