using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Users;
using Voltiq.Domain.Common;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;
        var document = Document.Create(request.Document).Value;

        var userAlreadyExists = await userRepository.ExistsUserAsync(
            document, email, cancellationToken);

        if (userAlreadyExists)
            return Result<CreateUserResponse>.Failure(new ConflictError(ResourceErrorMessages.USUARIO_EMAIL_JA_CADASTRADO));

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Create(request.Name, email, document, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = tokenService.GenerateToken(user.Id.ToString(), user.Name, []);

        return Result<CreateUserResponse>.Success(user.ToCreateUserResponse(token));
    }
}
