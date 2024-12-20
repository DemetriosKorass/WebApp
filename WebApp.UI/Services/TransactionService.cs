using Microsoft.EntityFrameworkCore;
using WebApp.DAL;

namespace WebApp.UI.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateMultipleEntitiesAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var task = await _context.Tasks.OrderBy(r => r.Id).FirstOrDefaultAsync();
                    task.Name = "!TRANSACTIONED!";
                    _context.Tasks.Update(task);

                    var user = await _context.Users.OrderBy(u => u.Id).FirstOrDefaultAsync();
                    user.Name = "!TRANSACTIONED!";
                    _context.Users.Update(user);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}