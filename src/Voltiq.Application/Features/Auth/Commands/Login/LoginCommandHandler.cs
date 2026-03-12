using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    public async Task<ErrorOr<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Error.Unauthorized(description: ResourceErrorMessages.LOGIN_CREDENCIAIS_INVALIDAS);

        var accessToken = tokenService.GenerateAccessToken(user.Id.ToString(), user.Name, []);
        var rawRefreshToken = tokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(rawRefreshToken, user.Id, expiresInDays: 7);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, rawRefreshToken);
    }
}
