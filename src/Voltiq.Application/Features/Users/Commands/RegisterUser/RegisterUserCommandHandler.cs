using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Users;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<RegisterUserCommand, ErrorOr<RegisterUserResponse>>
{
    public async Task<ErrorOr<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;
        var document = Document.Create(request.Document).Value;

        var userAlreadyExists = await userRepository.ExistsUserAsync(
            document, email, cancellationToken);

        if (userAlreadyExists)
            return Error.Conflict(description: ResourceErrorMessages.USUARIO_EMAIL_JA_CADASTRADO);

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Register(request.Name, email, document, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = tokenService.GenerateAccessToken(user.Id.ToString(), user.Name, []);

        return user.ToRegisterUserResponse(token);
    }
}
