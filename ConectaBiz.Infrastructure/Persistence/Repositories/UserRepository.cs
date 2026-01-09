using ConectaBiz.Application.DTOs;
using ConectaBiz.Domain.Entities;
using ConectaBiz.Domain.Interfaces;
using ConectaBiz.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ConectaBiz.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(c => c.Persona)
                .Include(c => c.Socio)
                .Where(c => c.Activo)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<User>> GetAllUsuarioByIdSocio(int idSocio)
        {
            return await _context.Users
                .Where(c => c.Activo && c.IdSocio == idSocio)
                .Include(c => c.Persona)
                .Include(c => c.Socio)
                .Where(c => c.Activo)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                 .Include(u => u.Socio)
                 .Include(u => u.Persona)
                                 .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Socio)
                .Include(u => u.Persona)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Persona.Correo == email);
        }


        public async Task<IEnumerable<User>> GetUsersByIdAsync(int[] ids)
        {
            return await _context.Users
                .Include(u => u.Socio)
                .Include(u => u.Persona)
                .Include(u => u.Rol)
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();
        }


        public async Task<User?> GetByIdSocioIdRolIdPersonaAsync(int idsocio, int idrol, int idPersona)
        {
            return await _context.Users
                .Include(u => u.Socio)
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.IdPersona == idPersona && u.IdSocio == idsocio && u.IdRol == idrol);
        }
        public async Task<IEnumerable<Rol>> GetAllRolAsync()
        {
            return await _context.Roles
                .Include(c => c.Users)
                .Where(c => c.Activo)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Rol> GetRolByIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<Rol> GetRolByCodigoAsync(string codigo)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(u => u.Codigo == codigo);
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                 .Include(u => u.Socio)
                 .Include(u => u.Persona)
                 .Include(u => u.Rol)
                 .Where(c => c.Activo)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        
        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            // Eliminación lógica
            user.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
        // En ConectaBiz.Infrastructure.Persistence.Repositories.UserRepository

        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token)
        {
            return await _context.PasswordResetToken
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token && !prt.IsUsed);
        }

        public async Task AddPasswordResetTokenAsync(PasswordResetToken token)
        {
            _context.PasswordResetToken.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task MarkPasswordResetTokenAsUsedAsync(string token)
        {
            var resetToken = await _context.PasswordResetToken
                .FirstOrDefaultAsync(prt => prt.Token == token);

            if (resetToken != null)
            {
                resetToken.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EmailVerificationCode?> GetEmailVerificationCodeAsync(string email, string code)
        {
            return await _context.EmailVerificationCode
                .Include(evc => evc.User)
                .FirstOrDefaultAsync(evc =>
                    evc.Email == email &&
                    evc.Code == code &&
                    !evc.IsUsed);
        }

        public async Task AddEmailVerificationCodeAsync(EmailVerificationCode verificationCode)
        {
            _context.EmailVerificationCode.Add(verificationCode);
            await _context.SaveChangesAsync();
        }

        public async Task MarkEmailVerificationCodeAsUsedAsync(int id)
        {
            var code = await _context.EmailVerificationCode.FindAsync(id);
            if (code != null)
            {
                code.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task InvalidateOldVerificationCodesAsync(string email)
        {
            var oldCodes = await _context.EmailVerificationCode
                .Where(evc => evc.Email == email && !evc.IsUsed)
                .ToListAsync();

            foreach (var code in oldCodes)
            {
                code.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}