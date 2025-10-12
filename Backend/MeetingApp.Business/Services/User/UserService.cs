using MeetingApp.Business.Abstractions.Email;
using MeetingApp.Business.Abstractions.User;
using MeetingApp.Business.Mappings;
using MeetingApp.DataAccess.Repositories.UnitOfWork;
using MeetingApp.Models.DTOs.User;
using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.EntityFrameworkCore;
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


        public UserService(IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordService passwordService,IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _emailService = emailService;
        }

        public async Task<IResult> Register(RegisterDto dto)
        {
            try
            {
                var email = dto.Email.ToLower().Trim();
                if (await _unitOfWork.Users.GetAllQueryable().AnyAsync(u => u.Email == email))
                    return new ErrorResult("Email zaten kullanımda.");

                if (!_passwordService.IsPasswordValid(dto.Password))
                    return new ErrorResult("Şifre en az 8 karakter, büyük/küçük harf, rakam ve özel karakter içermelidir.");

                var passwordHash = _passwordService.HashPassword(dto.Password);
                var user = UserMapper.ToEntity(dto, passwordHash);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var token = _jwtService.GenerateAccessToken(user.Id, user.Email, $"{user.FirstName} {user.LastName}");
                var resultDto = UserMapper.ToDto(user);

                 _emailService.QueueWelcomeEmail(user.Email, $"{user.FirstName} {user.LastName}");

                return new SuccessDataResult<UserDto>(resultDto, token);
            }
            catch (DbUpdateException ex)
            {
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Kayıt sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> Login(LoginDto dto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email.ToLower().Trim());

                if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                    return new ErrorResult("Email veya şifre hatalı.");

                var token = _jwtService.GenerateAccessToken(user.Id, user.Email, $"{user.FirstName} {user.LastName}");

                return new SuccessDataResult<UserLoginSuccessDto>(
                    new UserLoginSuccessDto { AccessToken = token },
                    "Giriş başarılı."
                );
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Giriş sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> GetProfile(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetAllQueryable().FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return new ErrorResult("Kullanıcı bulunamadı.");

                var resultDto = UserMapper.ToDto(user);

                return new SuccessDataResult<UserDto>(resultDto, "Profil bilgisi getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Profil getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _unitOfWork.Users.GetAllQueryable().FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return new ErrorResult("Kullanıcı bulunamadı.");

                if (!_passwordService.VerifyPassword(oldPassword, user.PasswordHash))
                    return new ErrorResult("Mevcut şifre yanlış.");

                if (!_passwordService.IsPasswordValid(newPassword))
                    return new ErrorResult("Yeni şifre en az 8 karakter, büyük/küçük harf, rakam ve özel karakter içermelidir.");

                user.PasswordHash = _passwordService.HashPassword(newPassword);

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return new SuccessResult("Şifre başarıyla değiştirildi.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ErrorResult("Kullanıcı başka bir işlem tarafından değiştirildi. Lütfen tekrar deneyin.");
            }
            catch (DbUpdateException ex)
            {
                return new ErrorResult($"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Şifre değiştirme sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }
}