using System;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Services;

public interface IParticipantService
{
    Task<List<Participant>> GetParticipantsAsync();
    Task<List<Participant>> GetParticipantsAsync(Guid raceId);
    Task<List<Participant>> GetParticipantsAsync(string raceCode);
    Task<Participant> GetParticipantAsync(string id, bool includeRace = false);
    Task<Participant> GetParticipantAsync(Guid participantId, bool includeRace = false);
    Task<Participant> GetParticipantWithCheckinsAsync(Guid participantId, bool includeRace = false);
    Task<Participant> AddOrUpdateParticipantAsync(Participant participant);
    Task<Participant> AddParticipantAsync(Participant participant);
    Task<Participant> UpdateParticipantAsync(Participant participant);
    Task<Participant> SetParticipantStatusAsync(Guid participantId, Status status);
    Task<Participant> LinkParticipantToUserIdAsync(Guid participantId, string userId, string profileImageUrl = "");
    Task UpdateParticipantProfileImageUrlAsync(Guid participantId, string profileImageUrl);
    Task<Participant?> GetParticipantByUserIdAsync(string userId);
}

public class ParticipantService : IParticipantService
{
    private readonly TrackerContext _context;
    private readonly ILeaderService _leaderService;

    public ParticipantService(TrackerContext context, ILeaderService leaderService)
    {
        _context = context;
        _leaderService = leaderService;
    }

    public async Task<List<Participant>> GetParticipantsAsync()
    {
        return await _context.Participants.Where(x => x.Race!.Active == true).ToListAsync();
    }

    public async Task<List<Participant>> GetParticipantsAsync(Guid raceId)
    {
        return await _context.Participants.Where(x => x.RaceId == raceId).ToListAsync();
    }

    public async Task<List<Participant>> GetParticipantsAsync(string raceCode)
    {
        return await _context.Participants.Where(x => x.Race!.Code == raceCode).ToListAsync();
    }

    public async Task<Participant> GetParticipantAsync(Guid participantId, bool includeRace = false)
    {
        if (includeRace)
        {
            return await _context.Participants.Where(x => x.Id == participantId).Include(x => x.Race).SingleAsync();
        }

        return await _context.Participants.Where(x => x.Id == participantId).SingleAsync();
    }

    public async Task<Participant> GetParticipantAsync(string id, bool includeRace = false)
    {
        if (Guid.TryParse(id, out var participantId))
        {
            return await GetParticipantAsync(participantId, includeRace);
        }

        // TODO: Clean this up
        if (includeRace)
        {
            return await _context.Participants.Where(x => x.Bib == id).Include(x => x.Race).SingleAsync();
        }

        return await _context.Participants.Where(x => x.Bib == id).SingleAsync();
    }

    public async Task<Participant> GetParticipantWithCheckinsAsync(Guid participantId, bool includeRace = false)
    {
        var query = _context.Participants.Where(x => x.Id == participantId);

        if (includeRace)
        {
            query = query.Include(x => x.Race);
        }

        return await query.Include(x => x.Checkins).SingleAsync();
    }

    public async Task<Participant> AddOrUpdateParticipantAsync(Participant participant)
    {
        Participant _participant = new Participant();

        if (await _context.Participants.AnyAsync(x => x.FirstName == participant.FirstName && x.LastName == participant.LastName && x.City == participant.City && x.Region == participant.Region))
        {
            _participant =  await UpdateParticipantAsync(participant);
        }
        else
        {
            _participant = await AddParticipantAsync(participant);
        }

        await _leaderService.CreateLeaderAsync(_participant.Id);
        return _participant;           
    }

    public async Task<Participant> AddParticipantAsync(Participant participant)
    {
        participant.Id = Guid.NewGuid();
        await _context.Participants.AddAsync(participant);
        await _context.SaveChangesAsync();
        return participant;
    }

    public async Task<Participant> UpdateParticipantAsync(Participant participant)
    {
        var p = await _context.Participants.Where(x => x.FirstName == participant.FirstName && x.LastName == participant.LastName && x.City == participant.City && x.Region == participant.Region).FirstAsync();
        p.Region = participant.Region;
        p.City = participant.City;
        p.Age = participant.Age;
        p.Bib = participant.Bib;
        p.Gender = participant.Gender;
        p.RaceId = participant.RaceId;
        p.Rank = participant.Rank;

        await _context.SaveChangesAsync();
        
        return p;
    }

    public async Task<Participant> SetParticipantStatusAsync(Guid participantId, Status status)
    {
        var participant = await GetParticipantAsync(participantId);
        participant.Status = Status.Finished;
        await _context.SaveChangesAsync();
        return participant;
    }

    public async Task<Participant> LinkParticipantToUserIdAsync(Guid participantId, string userId, string profileImageUrl = "")
    {
        var participant = await GetParticipantAsync(participantId);
        participant.UserId = userId;
        if (!String.IsNullOrEmpty(profileImageUrl))
        {
            participant.PictureUrl = profileImageUrl;
        }
        await _context.SaveChangesAsync();
        return participant;
    }

    public async Task UpdateParticipantProfileImageUrlAsync(Guid participantId, string profileImageUrl)
    {
        var participant = await GetParticipantAsync(participantId);
        
        if (!String.IsNullOrEmpty(profileImageUrl))
        {
            participant.PictureUrl = profileImageUrl;
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<Participant?> GetParticipantByUserIdAsync(string userId)
    {
        return await _context.Participants.SingleOrDefaultAsync(p => p.UserId == userId);
    }
}