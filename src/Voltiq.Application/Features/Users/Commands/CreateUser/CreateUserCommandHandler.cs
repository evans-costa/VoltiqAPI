using MediatR;
using Voltiq.Domain.Common;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.ValueObjects;
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
        var requestEmail = Email.Create(request.Email);
        var requestDocument = Document.Create(request.Document);

        var existingByEmail = await userRepository.FindAsync(
            u => u.Email == requestEmail, cancellationToken);

        if (existingByEmail.Any())
            return Result.Failure<Guid>(ResourceErrorMessages.USUARIO_EMAIL_JA_CADASTRADO);

        var existingByDocument = await userRepository.FindAsync(
            u => u.Document == requestDocument, cancellationToken);

        if (existingByDocument.Any())
            return Result.Failure<Guid>(ResourceErrorMessages.USUARIO_DOCUMENTO_JA_CADASTRADO);

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Create(request.Name, request.Email, request.Document, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
