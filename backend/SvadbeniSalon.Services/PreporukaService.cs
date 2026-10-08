using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public interface IPreporukaService
{
    Task<UserInterestsResponse> GetInterestsAsync(int userId);
    Task<UserInterestsResponse> SetInterestsAsync(int userId, UserInterestsRequest request);
    Task<List<PreporukaResponse>> GetRecommendationsAsync(int userId, int limit = 5);
}

public class PreporukaService : IPreporukaService
{
    private readonly SvadbeniSalonDbContext _context;
    private const int MinCollaborativeRating = 4;

    public PreporukaService(SvadbeniSalonDbContext context)
    {
        _context = context;
    }

    public async Task<UserInterestsResponse> GetInterestsAsync(int userId)
    {
        var zanrovi = await _context.UserZanrovi
            .AsNoTracking()
            .Where(uz => uz.UserId == userId)
            .Include(uz => uz.Zanr)
            .Select(uz => uz.Zanr)
            .Where(z => z.IsActive)
            .OrderBy(z => z.Naziv)
            .ToListAsync();

        return new UserInterestsResponse
        {
            ZanrIds = zanrovi.Select(z => z.Id).ToList(),
            Zanrovi = zanrovi.Select(z => new ZanrResponse
            {
                Id = z.Id,
                Naziv = z.Naziv,
                IsActive = z.IsActive,
                CreatedAt = z.CreatedAt,
            }).ToList(),
        };
    }

    public async Task<UserInterestsResponse> SetInterestsAsync(int userId, UserInterestsRequest request)
    {
        var zanrIds = (request.ZanrIds ?? new List<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (zanrIds.Count == 0)
        {
            throw new ClientException("Odaberite barem jednu interesnu skupinu (žanr).");
        }

        var existingCount = await _context.Zanrovi
            .CountAsync(z => zanrIds.Contains(z.Id) && z.IsActive);
        if (existingCount != zanrIds.Count)
        {
            throw new ClientException("Jedan ili više odabranih žanrova ne postoji.");
        }

        var current = await _context.UserZanrovi
            .Where(uz => uz.UserId == userId)
            .ToListAsync();
        _context.UserZanrovi.RemoveRange(current);

        foreach (var zanrId in zanrIds)
        {
            _context.UserZanrovi.Add(new UserZanr { UserId = userId, ZanrId = zanrId });
        }

        await _context.SaveChangesAsync();
        return await GetInterestsAsync(userId);
    }

    public async Task<List<PreporukaResponse>> GetRecommendationsAsync(int userId, int limit = 5)
    {
        limit = Math.Clamp(limit, 1, 20);

        var userZanrIds = await _context.UserZanrovi
            .AsNoTracking()
            .Where(uz => uz.UserId == userId)
            .Select(uz => uz.ZanrId)
            .ToListAsync();

        var zanrNames = await _context.Zanrovi
            .AsNoTracking()
            .Where(z => userZanrIds.Contains(z.Id))
            .ToDictionaryAsync(z => z.Id, z => z.Naziv);

        var reviewedPonudaIds = await _context.Recenzije
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.PonudaId)
            .Distinct()
            .ToListAsync();

        var ponude = await _context.Ponude
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.MuzicariPonuda)
                .ThenInclude(mp => mp.Muzicar)
                    .ThenInclude(m => m.MuzicarZanrovi)
            .ToListAsync();

        var allReviews = await _context.Recenzije
            .AsNoTracking()
            .Select(r => new { r.UserId, r.PonudaId, r.Ocjena })
            .ToListAsync();

        var avgByPonuda = allReviews
            .GroupBy(r => r.PonudaId)
            .ToDictionary(
                g => g.Key,
                g => g.Average(x => x.Ocjena));

        var myHighRated = allReviews
            .Where(r => r.UserId == userId && r.Ocjena >= MinCollaborativeRating)
            .Select(r => r.PonudaId)
            .ToHashSet();

        var similarUserIds = allReviews
            .Where(r =>
                r.UserId != userId &&
                r.Ocjena >= MinCollaborativeRating &&
                myHighRated.Contains(r.PonudaId))
            .Select(r => r.UserId)
            .Distinct()
            .ToHashSet();

        // Ako trenutni korisnik još nema ocjena, sličnost = isti interesi
        if (similarUserIds.Count == 0 && userZanrIds.Count > 0)
        {
            var usersWithSharedInterests = await _context.UserZanrovi
                .AsNoTracking()
                .Where(uz => uz.UserId != userId && userZanrIds.Contains(uz.ZanrId))
                .Select(uz => uz.UserId)
                .Distinct()
                .ToListAsync();
            similarUserIds = usersWithSharedInterests.ToHashSet();
        }

        var collaborativeBoost = allReviews
            .Where(r =>
                similarUserIds.Contains(r.UserId) &&
                r.Ocjena >= MinCollaborativeRating &&
                !reviewedPonudaIds.Contains(r.PonudaId))
            .GroupBy(r => r.PonudaId)
            .ToDictionary(
                g => g.Key,
                g => (Score: g.Average(x => x.Ocjena), Count: g.Count()));

        var results = new List<PreporukaResponse>();

        foreach (var ponuda in ponude)
        {
            if (reviewedPonudaIds.Contains(ponuda.Id) && myHighRated.Contains(ponuda.Id))
            {
                // Već ocijenio visoko — manje prioritet za "preporuku", ali i dalje može biti u listi ako treba
            }

            var packageZanrIds = ponuda.MuzicariPonuda
                .SelectMany(mp => mp.Muzicar.MuzicarZanrovi.Select(mz => mz.ZanrId))
                .Distinct()
                .ToList();

            var matching = packageZanrIds
                .Where(id => userZanrIds.Contains(id))
                .Select(id => zanrNames.GetValueOrDefault(id, $"#{id}"))
                .Distinct()
                .ToList();

            var interestScore = userZanrIds.Count == 0
                ? 0
                : (double)matching.Count / userZanrIds.Count;

            collaborativeBoost.TryGetValue(ponuda.Id, out var collab);
            var collabScore = collab.Count > 0
                ? (collab.Score / 5.0) * (1 + Math.Log(1 + collab.Count))
                : 0;

            avgByPonuda.TryGetValue(ponuda.Id, out var avgRating);
            var popularityScore = avgRating > 0 ? avgRating / 5.0 : 0.2;

            var score = (interestScore * 0.55) + (collabScore * 0.30) + (popularityScore * 0.15);
            if (score <= 0)
            {
                continue;
            }

            var razlog = BuildReason(matching, collab.Count, avgRating, userZanrIds.Count > 0);

            results.Add(new PreporukaResponse
            {
                PonudaId = ponuda.Id,
                Naziv = ponuda.Naziv,
                Opis = ponuda.Opis,
                Cijena = ponuda.Cijena,
                Score = Math.Round(score, 3),
                Razlog = razlog,
                MatchingZanrovi = matching,
            });
        }

        if (results.Count == 0)
        {
            // Fallback: aktivne ponude po popularnosti
            results = ponude
                .Select(p =>
                {
                    avgByPonuda.TryGetValue(p.Id, out var avg);
                    return new PreporukaResponse
                    {
                        PonudaId = p.Id,
                        Naziv = p.Naziv,
                        Opis = p.Opis,
                        Cijena = p.Cijena,
                        Score = Math.Round(avg > 0 ? avg / 5.0 : 0.1, 3),
                        Razlog = avg > 0
                            ? $"Popularan paket (prosječna ocjena {avg:0.0})"
                            : "Aktivna ponuda salona",
                        MatchingZanrovi = new List<string>(),
                    };
                })
                .ToList();
        }

        return results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Naziv)
            .Take(limit)
            .ToList();
    }

    private static string BuildReason(
        List<string> matching,
        int similarRatingsCount,
        double avgRating,
        bool hasInterests)
    {
        var parts = new List<string>();

        if (matching.Count > 0)
        {
            parts.Add($"Odgovara vašim interesima: {string.Join(", ", matching)}");
        }
        else if (!hasInterests)
        {
            parts.Add("Preporuka na osnovu popularnosti (postavite interese za personalizaciju)");
        }

        if (similarRatingsCount > 0)
        {
            parts.Add(
                similarRatingsCount == 1
                    ? "Korisnici sa sličnim ukusom su ocijenili ovaj paket visoko"
                    : $"Korisnici sa sličnim ukusom ({similarRatingsCount} ocjena) preporučuju ovaj paket");
        }

        if (avgRating >= 4 && parts.Count < 2)
        {
            parts.Add($"Visoka prosječna ocjena ({avgRating:0.0}/5)");
        }

        return parts.Count > 0
            ? string.Join(". ", parts) + "."
            : "Preporučeno na osnovu dostupnih ponuda.";
    }
}
