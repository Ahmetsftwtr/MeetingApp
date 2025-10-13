using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Business.Abstractions.File;
using MeetingApp.Business.Abstractions.User;
using MeetingApp.Business.Mappings;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.DTOs.User;
using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MeetingApp.Business.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IPasswordService passwordService,
            IFileService fileService,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _emailService = emailService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<IResult> Register(RegisterDto dto, FileUploadDto? profileImageDto)
        {
            string? uploadedFilePath = null;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("User registration started for email: {Email}", dto.Email);

                var email = dto.Email.ToLower().Trim();
                if (await _unitOfWork.Users.GetAllQueryable().AnyAsync(u => u.Email == email))
                {
                    _logger.LogWarning("Registration failed: Email already in use - {Email}", email);
                    return new ErrorResult("Email zaten kullanımda.");
                }

                if (!_passwordService.IsPasswordValid(dto.Password))
                {
                    _logger.LogWarning("Registration failed: Invalid password format for {Email}", email);
                    return new ErrorResult("Şifre en az 8 karakter, büyük/küçük harf, rakam ve özel karakter içermelidir.");
                }

                var passwordHash = _passwordService.HashPassword(dto.Password);
                var user = UserMapper.ToEntity(dto, passwordHash);

                if (profileImageDto != null)
                {
                    _logger.LogInformation("Uploading profile image for user: {Email}", email);

                    var uploadResult = await _fileService.UploadFileAsync(profileImageDto, "profile");
                    if (uploadResult.IsSuccess)
                    {
                        var uploadData = ((SuccessDataResult<FileUploadResultDto>)uploadResult).Data;
                        user.ProfileImagePath = uploadData.FilePath;
                        uploadedFilePath = uploadData.FilePath;

                        _logger.LogInformation("Profile image uploaded successfully: {FilePath}", uploadedFilePath);
                    }
                    else
                    {
                        _logger.LogWarning("Profile image upload failed for {Email}: {Message}", email, uploadResult.Message);
                    }
                }

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("User registered successfully: {UserId} - {Email}", user.Id, user.Email);

                var token = _jwtService.GenerateAccessToken(user.Id, user.Email, $"{user.FirstName} {user.LastName}");
                var resultDto = UserMapper.ToDto(user);

                _emailService.QueueWelcomeEmail(user.Email, $"{user.FirstName} {user.LastName}");

                return new SuccessDataResult<UserDto>(resultDto, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration for email: {Email}", dto.Email);

                await transaction.RollbackAsync();

                if (uploadedFilePath != null)
                {
                    try
                    {
                        _logger.LogInformation("Cleaning up uploaded file due to registration failure: {FilePath}", uploadedFilePath);
                        await _fileService.DeleteFileAsync(uploadedFilePath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Failed to cleanup uploaded file: {FilePath}", uploadedFilePath);
                    }
                }

                return new ErrorResult($"Kayıt sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> Login(LoginDto dto)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

                var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email.ToLower().Trim());

                if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid credentials for {Email}", dto.Email);
                    return new ErrorResult("Email veya şifre hatalı.");
                }

                var token = _jwtService.GenerateAccessToken(user.Id, user.Email, $"{user.FirstName} {user.LastName}");
                var resultDto = UserMapper.ToDto(user);

                _logger.LogInformation("User logged in successfully: {UserId} - {Email}", user.Id, user.Email);

                return new SuccessDataResult<UserDto>(resultDto, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", dto.Email);
                return new ErrorResult($"Giriş sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetProfile(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetAllQueryable().FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("Profile not found for user: {UserId}", userId);
                    return new ErrorResult("Kullanıcı bulunamadı.");
                }

                var resultDto = UserMapper.ToDto(user);

                return new SuccessDataResult<UserDto>(resultDto, "Profil bilgisi getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting profile for user: {UserId}", userId);
                return new ErrorResult($"Profil getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            try
            {
                _logger.LogInformation("Password change requested for user: {UserId}", userId);

                var user = await _unitOfWork.Users.GetAllQueryable().FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("Password change failed: User not found - {UserId}", userId);
                    return new ErrorResult("Kullanıcı bulunamadı.");
                }

                if (!_passwordService.VerifyPassword(oldPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Password change failed: Invalid old password for user {UserId}", userId);
                    return new ErrorResult("Mevcut şifre yanlış.");
                }

                if (!_passwordService.IsPasswordValid(newPassword))
                {
                    _logger.LogWarning("Password change failed: Invalid new password format for user {UserId}", userId);
                    return new ErrorResult("Yeni şifre en az 8 karakter, büyük/küçük harf, rakam ve özel karakter içermelidir.");
                }

                user.PasswordHash = _passwordService.HashPassword(newPassword);

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                return new SuccessResult("Şifre başarıyla değiştirildi.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error during password change for user: {UserId}", userId);
                return new ErrorResult("Kullanıcı başka bir işlem tarafından değiştirildi. Lütfen tekrar deneyin.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during password change for user: {UserId}", userId);
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password change for user: {UserId}", userId);
                return new ErrorResult($"Şifre değiştirme sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }
}