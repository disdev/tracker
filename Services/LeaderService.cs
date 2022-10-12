using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;
using Tracker.Utilities;

namespace Tracker.Services;

public interface ILeaderService
{
    Task<List<Leader>> GetLeadersAsync();
    Task<List<Leader>> GetLeadersByRaceAsync(Guid raceId);
    Task<List<Leader>> GetLeadersByWatcherUserAsync(string userId);
    Task CreateLeaderAsync(Guid participantId);
    Task<bool> LeaderExistsAsync(Guid participantId);
    Task<Leader> UpdateLeaderAsync(Checkin checkin, Segment segment);
    Task<List<Leader>> GetLeadersByDroppedStatusAsync();
}

public class LeaderService : ILeaderService
{
    private readonly TrackerContext _context;

    public LeaderService(TrackerContext context)
    {
        _context = context;
    }

    public async Task<List<Leader>> GetLeadersAsync()
    {
        return await _context.Leaders
            .Include(l => l.Checkin)
            .Include(l => l.Segment)
            .Include(l => l.Participant)
            .Where(l => l.Participant.Race.Active == true)
            .OrderByDescending(l => l.Segment.TotalDistance)
            .ThenBy(l => l.Checkin.When)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Leader>> GetLeadersByRaceAsync(Guid raceId)
    {
        return await _context.Leaders
            .Include(l => l.Checkin)
            .Include(l => l.Segment)
            .Include(l => l.Participant)
            .AsNoTracking()
            .Where(l => l.Participant.Race.Active == true && l.Participant.RaceId == raceId)
            .OrderByDescending(l => l.Segment.TotalDistance)
            .ThenBy(l => l.Checkin.When)
        .ToListAsync();
    }

    public async Task<List<Leader>> GetLeadersByWatcherUserAsync(string userId)
    {
        var watchers = await _context.Watchers
            .Where(w => w.UserId == userId)
            .AsNoTracking()
            .Select(w => w.ParticipantId)
            .ToListAsync();

        return await _context.Leaders
            .Include(l => l.Checkin)
            .Include(l => l.Segment)
            .Include(l => l.Participant)
            .AsNoTracking()
            .Where(l => l.Participant.Race.Active == true && watchers.Contains(l.ParticipantId))
            .OrderByDescending(l => l.Segment.TotalDistance)
            .ThenBy(l => l.Checkin.When)
        .ToListAsync();
    }

    public async Task<bool> LeaderExistsAsync(Guid participantId)
    {
        return await _context.Leaders.AnyAsync(l => l.ParticipantId == participantId);
    }

    public async Task CreateLeaderAsync(Guid participantId)
    {
        if (!(await LeaderExistsAsync(participantId)))
        {
            await _context.Leaders.AddAsync(new Leader() {
                Id = Guid.NewGuid(),
                ParticipantId = participantId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Leader> UpdateLeaderAsync(Checkin checkin, Segment segment)
    {
        var leader = await _context.Leaders.Where(l => l.ParticipantId == checkin.ParticipantId).FirstAsync();
        leader.CheckinId = checkin.Id;
        leader.SegmentId = checkin.SegmentId;
        leader.CheckpointId = segment.ToCheckpointId;
        leader.OverallTime = (int)(checkin.When - checkin.Participant.Race.Start).TotalSeconds;
        leader.OverallPace = (int)TimeHelpers.CalculatePaceInSeconds(checkin.Participant.Race.Start, checkin.When, segment.TotalDistance);
        
        await _context.SaveChangesAsync();
        return leader;
    }

    public async Task<List<Leader>> GetLeadersByDroppedStatusAsync()
    {
        return await _context.Leaders
            .Include(l => l.Checkin)
            .Include(l => l.Segment)
            .Include(l => l.Participant)
            .Where(l => l.Participant.Race.Active == true && (l.Participant.Status == Status.DNF || l.Participant.Status == Status.DNS))
            .OrderBy(l => l.Participant.Bib)
            .ThenBy(l => l.Checkin.When)
            .AsNoTracking()
            .ToListAsync();
    }
}