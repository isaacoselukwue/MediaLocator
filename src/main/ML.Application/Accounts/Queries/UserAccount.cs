using MediatR;
using Microsoft.EntityFrameworkCore;
using ML.Application.Common.Interfaces;
using ML.Application.Common.Models;

namespace ML.Application.Accounts.Queries;

public record UserAccountQuery : IRequest<Result<List<UserAccountDto>>>
{
    public int PageNumber { get; set; }
    public int PageCount { get; set; }
}

public class UserAccountValidator : AbstractValidator<UserAccountQuery>
{
    public UserAccountValidator()
    {
        RuleFor(v => v.PageNumber).GreaterThan(0);
        RuleFor(v => v.PageCount).GreaterThan(0);
    }
}

internal class UserAccountQueryHandler(IIdentityService identityService) : IRequestHandler<UserAccountQuery, Result<List<UserAccountDto>>>
{
    public async Task<Result<List<UserAccountDto>>> Handle(UserAccountQuery request, CancellationToken cancellationToken)
    {
        var userAccounts = identityService.UserAccounts();
        var result = await userAccounts.Select(x => new UserAccountDto
        {
            DateAccountCreated = x.Created,
            EmailAddress = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName,
            PhoneNumber = x.PhoneNumber,
            UserId = x.Id
        })
            .Skip((request.PageNumber - 1) * request.PageCount)
            .Take(request.PageCount)
            .ToListAsync(cancellationToken: cancellationToken);
        return Result<List<UserAccountDto>>.Success("User accounts retrieved successfully.", result);
    }
}


public class UserAccountDto
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset DateAccountCreated { get; set; }
}