using FluentValidation;

namespace Billing.Application.Common.Invoices.Command.Create;

public sealed class IssueInvoiceValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
            l.RuleFor(x => x.Amount).GreaterThan(0);
        });
    }
}
