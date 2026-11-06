using Hotel_booking.Core.Models;
using Hotel_booking.Core.ModelsDTO.ModelsDtoResponse;
using Hotel_booking.Core.Options;
using Hotel_booking.Core.Services.Contract.TokenServices;
using Hotel_booking.Core.Specifications.RefreshTokenSpecifications;
using Hotel_booking.Core.UnitOfWorks;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_Booking.Services.TokenServices
{
    public class RefreshTokenService : IRefreshTokenService
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly JwtOptions _jwtOptions;

        public RefreshTokenService(IUnitOfWork unitOfWork, IOptions<JwtOptions> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _jwtOptions = jwtOptions.Value;
        }

        #region 1. Generate random token
        private string GenerateToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }
        #endregion

        #region 2. Hash token before saving=
        private string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        }
        #endregion

        #region 3. Create new refresh token
        public async Task<string> CreateRefreshTokenAsync(string userId)
        {
            var token = GenerateToken();

            var hashToken = HashToken(token);

            var refreshToken = new RefreshToken
            {
                TokenHash = hashToken,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            await _unitOfWork.CreateRepository<RefreshToken>().AddAsync(refreshToken);
            await _unitOfWork.CompleteAsync();

            return token;
        }
        #endregion

        #region 4. Validate and rotate refresh token
        public async Task<RefreshTokenRotationResult> RotateRefreshTokenAsync(string token)
        {
            var hashToken = HashToken(token);

            var spec = new RefreshTokenSpec(token);
            var tokenRepo = _unitOfWork.CreateRepository<RefreshToken>();
            var oldToken = await tokenRepo.GetAsyncSpec(spec);

            if (oldToken == null || !oldToken.IsActive)
                return null;

            var newRefteshToken = GenerateToken();

            var newHashToken = HashToken(newRefteshToken);

            oldToken.RevokedAt = DateTime.UtcNow;

            oldToken.ReplacedByTokenHash = newHashToken;

            var newToken = new RefreshToken()
            {
                TokenHash = newHashToken,
                UserId = oldToken.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            };

            await tokenRepo.AddAsync(newToken);
            await _unitOfWork.CompleteAsync();

            return new RefreshTokenRotationResult
            {
                RefreshToken = newRefteshToken,
                User = oldToken.User
            };
        }
        #endregion

        #region 5. Revoke refresh token
        public async Task RevokeRefreshTokenAsync(string token)
        {
            var hashToken = HashToken(token);

            var spec = new RefreshTokenSpec(token);
            var refreshToken = await _unitOfWork.CreateRepository<RefreshToken>().GetAsyncSpec(spec);
            if(refreshToken is null || !refreshToken.IsActive)
                return;

            refreshToken.RevokedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
        }
        #endregion
    }
}
