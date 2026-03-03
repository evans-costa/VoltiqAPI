using System.Text.RegularExpressions;
using MediatR;
using Voltiq.Domain.Common;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingByEmail = await userRepository.FindAsync(
            u => u.Email.Value == request.Email.Trim().ToLowerInvariant(),
            cancellationToken);

        if (existingByEmail.Any())
            return Result.Failure<Guid>(ResourceErrorMessages.Usuario_Email_JaCadastrado);

        var documentDigits = Regex.Replace(request.Document, @"\D", "");

        var existingByDocument = await userRepository.FindAsync(
            u => u.Document.Value == documentDigits,
            cancellationToken);

        if (existingByDocument.Any())
            return Result.Failure<Guid>(ResourceErrorMessages.Usuario_Documento_JaCadastrado);

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Create(request.Name, request.Email, request.Document, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
