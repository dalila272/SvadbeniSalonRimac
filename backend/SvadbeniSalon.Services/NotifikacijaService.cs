using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Messages;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public interface INotifikacijaService
{
    Task<NotifikacijaResponse> CreateFromMessageAsync(NotificationMessage message);
    Task<List<NotifikacijaResponse>> GetMineAsync(int userId, int? take = null);
    Task<int> GetUnreadCountAsync(int userId);
    Task<NotifikacijaResponse> MarkAsReadAsync(int userId, int notificationId);
    Task<int> MarkAllAsReadAsync(int userId);
}

public class NotifikacijaService : INotifikacijaService
{
    private readonly SvadbeniSalonDbContext _context;

    public NotifikacijaService(SvadbeniSalonDbContext context)
    {
        _context = context;
    }

    public async Task<NotifikacijaResponse> CreateFromMessageAsync(NotificationMessage message)
    {
        if (message.UserId <= 0)
        {
            throw new ClientException("Notifikacija mora imati korisnika.");
        }

        var entity = new Notifikacija
        {
            UserId = message.UserId,
            Naslov = Truncate(message.Title, 200),
            Tekst = Truncate(message.Body, 2000),
            Kind = string.IsNullOrWhiteSpace(message.Kind) ? null : Truncate(message.Kind, 50),
            IsRead = false,
            CreatedAt = message.CreatedAt == default ? DateTime.UtcNow : message.CreatedAt,
        };

        _context.Notifikacije.Add(entity);
        await _context.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<List<NotifikacijaResponse>> GetMineAsync(int userId, int? take = null)
    {
        var query = _context.Notifikacije
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsQueryable();

        if (take is > 0)
        {
            query = query.Take(take.Value);
        }

        var items = await query.ToListAsync();
        return items.Select(Map).ToList();
    }

    public Task<int> GetUnreadCountAsync(int userId) =>
        _context.Notifikacije.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<NotifikacijaResponse> MarkAsReadAsync(int userId, int notificationId)
    {
        var entity = await _context.Notifikacije
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (entity == null)
        {
            throw new NotFoundException("Notifikacija nije pronađena.");
        }

        if (!entity.IsRead)
        {
            entity.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return Map(entity);
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var unread = await _context.Notifikacije
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var item in unread)
        {
            item.IsRead = true;
        }

        if (unread.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return unread.Count;
    }

    private static NotifikacijaResponse Map(Notifikacija n) => new()
    {
        Id = n.Id,
        Naslov = n.Naslov,
        Tekst = n.Tekst,
        Kind = n.Kind,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
    };

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Length <= max
                ? value
                : value[..max];
}
