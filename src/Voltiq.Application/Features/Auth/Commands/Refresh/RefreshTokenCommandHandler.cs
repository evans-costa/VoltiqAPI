using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.RefreshToken;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenReadOnlyRepository refreshTokenReadOnlyRepository,
    IRefreshTokenWriteOnlyRepository refreshTokenWriteOnlyRepository,
    IUserReadOnlyRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthResponse>>
{
    public async Task<ErrorOr<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await refreshTokenReadOnlyRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken is null)
            return Error.Unauthorized(description: ResourceErrorMessages.REFRESH_TOKEN_NAO_ENCONTRADO);

        if (refreshToken.IsExpired)
            return Error.Unauthorized(description: ResourceErrorMessages.REFRESH_TOKEN_EXPIRADO);

        if (!refreshToken.IsActive)
            return Error.Unauthorized(description: ResourceErrorMessages.REFRESH_TOKEN_INVALIDO);

        var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);

        if (user is null)
            return Error.Unauthorized(description: ResourceErrorMessages.REFRESH_TOKEN_INVALIDO);

        refreshToken.Revoke();

        var newAccessToken = tokenService.GenerateAccessToken(user.Id.ToString(), user.Name, []);
        var newRawRefreshToken = tokenService.GenerateRefreshToken();

        var newRefreshToken = RefreshToken.Create(newRawRefreshToken, user.Id, expiresInDays: 7);
        await refreshTokenWriteOnlyRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRawRefreshToken);
    }
}
