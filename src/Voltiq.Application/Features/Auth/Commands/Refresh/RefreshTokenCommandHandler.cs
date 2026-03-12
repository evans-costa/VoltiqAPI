using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Common;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken is null)
            return Result<AuthResponse>.Failure(
                new UnauthorizedError(ResourceErrorMessages.REFRESH_TOKEN_NAO_ENCONTRADO));

        if (refreshToken.IsExpired)
            return Result<AuthResponse>.Failure(
                new UnauthorizedError(ResourceErrorMessages.REFRESH_TOKEN_EXPIRADO));

        if (!refreshToken.IsActive)
            return Result<AuthResponse>.Failure(
                new UnauthorizedError(ResourceErrorMessages.REFRESH_TOKEN_INVALIDO));

        var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);

        if (user is null)
            return Result<AuthResponse>.Failure(
                new UnauthorizedError(ResourceErrorMessages.REFRESH_TOKEN_INVALIDO));

        refreshToken.Revoke();

        var newAccessToken = tokenService.GenerateAccessToken(user.Id.ToString(), user.Name, []);
        var newRawRefreshToken = tokenService.GenerateRefreshToken();

        var newRefreshToken = RefreshToken.Create(newRawRefreshToken, user.Id, expiresInDays: 7);
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(newAccessToken, newRawRefreshToken));
    }
}
