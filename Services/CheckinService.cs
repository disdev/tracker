using System;
using Microsoft.EntityFrameworkCore;
using Tracker.Models;
using Tracker.Data;
using Tracker.Utilities;

namespace Tracker.Services;

public interface ICheckinService
{
    Task<List<Checkin>> GetCheckinsAsync();
    Task<List<Checkin>> GetCheckinsAsync(Guid raceId);
    Task<List<Checkin>> GetUnconfirmedCheckinsAsync();
    Task<Checkin> GetCheckinAsync(Guid checkinId);
    Task<Checkin?> GetCheckinAsync(Guid participantId, int order);
    Task<List<Checkin>> GetCheckinsForParticipantAsync(Guid participantId);
    Task<List<Checkin>> GetCheckinsForSegmentAsync(Guid segmentId);
    Task<Int16> HandleCheckinsAsync(Message message);
    Task<Int16> HandleCheckinAsync(string bib, string monitorPhoneNumber, Guid messageId);
    Task<Checkin> ConfirmCheckinAsync(Guid checkinId, DateTime? when = null, Guid? segmentId = null);
    Task<Checkin> GetLastCheckinForParticipant(Guid participantId);
    Task<List<Checkin>> GetCheckinsForCheckpointAsync(Guid checkpointId);
}

public class CheckinService : ICheckinService
{
    private readonly TrackerContext _context;
    private readonly IParticipantService _participantService;
    private readonly IMonitorService _monitorService;
    private readonly IWatcherService _watcherService;
    private readonly ISegmentService _segmentService;
    // private readonly ITwilioService _twilioService;
    private readonly ILeaderService _leaderService;
    private readonly SlackService _slackService;

    public CheckinService(TrackerContext context, IParticipantService participantService, IMonitorService monitorService, IWatcherService watcherService, ISegmentService segmentService, ILeaderService leaderService, SlackService slackService)
    {
        _context = context;
        _participantService = participantService;
        _monitorService = monitorService;
        _watcherService = watcherService;
        _segmentService = segmentService;
        // _twilioService = twilioService;
        _leaderService = leaderService;
        _slackService = slackService;
    }

    public async Task<List<Checkin>> GetCheckinsAsync()
    {
        return await _context.Checkins
            .OrderBy(x => x.When)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Checkin>> GetCheckinsAsync(Guid raceId)
    {
        return await _context.Checkins
            .Where(x => x.Participant.RaceId == raceId)
            .Include(x => x.Segment)
            .OrderBy(x => x.When)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Checkin>> GetUnconfirmedCheckinsAsync()
    {
        return await _context.Checkins
            .Where(x => x.Confirmed == false)
            .Include(x => x.Participant)
            .Include(x=> x.Segment)
            .Include(x => x.Message)
            .OrderBy(x => x.When)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Checkin> GetCheckinAsync(Guid checkinId)
    {
        return await _context.Checkins.Where(x => x.Id == checkinId).FirstAsync();
    }

    public async Task<Checkin?> GetCheckinAsync(Guid participantId, int order)
    {
        return await _context.Checkins.FirstOrDefaultAsync(ci => ci.ParticipantId == participantId && ci.Segment.Order == order);
    }

    public async Task<List<Checkin>> GetCheckinsForParticipantAsync(Guid participantId)
    {
        return await _context.Checkins
            .Where(x => x.ParticipantId == participantId)
            .OrderBy(x => x.Segment.Order)
            .Include(x => x.Segment)
            .ToListAsync();
    }

    public async Task<List<Checkin>> GetCheckinsForSegmentAsync(Guid segmentId)
    {
        return await _context.Checkins
            .Where(ci => ci.SegmentId == segmentId)
            .Where(x => x.Confirmed == true)
            .Include(x => x.Participant)
            .Include(x => x.Message)
            .Include(x => x.Segment)
            .ToListAsync();
    }

    public async Task<Int16> HandleCheckinsAsync(Message message)
    {
        var messageParts = message.Body.Trim().Split(' ');
        Int16 checkinCount = 0;

        foreach (var part in messageParts)
        {
            checkinCount += await HandleCheckinAsync(part, message.From, message.Id);
        }

        return checkinCount;
    }

    public async Task<Int16> HandleCheckinAsync(string bib, string monitorPhoneNumber, Guid messageId)
    {
        var participant = await _participantService.GetParticipantAsync(bib, true);
        var checkins = await GetCheckinsForParticipantAsync(participant.Id);
        var monitors = await _monitorService.GetMonitorsForPhoneNumberAsync(monitorPhoneNumber);
        var segments = await _segmentService.GetSegmentsAsync(participant.RaceId);
        var checkin = new Checkin() { Id = Guid.Empty };
        int checkinSegmentOrder = -1;
        var checkinTime = DateTime.UtcNow;

        if (participant.Status != Status.Started)
        {
            throw new InvalidDataException("Participant has not started the race but a checkin is being recorded!");
        }

        var lastCheckinTime = checkins.Count == 0 ? participant.Race.Start : checkins.Last().When;
        var skipIndex = checkins.Count == 0 ? 0 : (segments.FindIndex(x => x.Id == checkins.Last().SegmentId) + 1);
        var futureSegments = segments.Skip(skipIndex);

        foreach (var segment in futureSegments)
        {
            if (monitors.Any(x => x.CheckpointId == segment.ToCheckpointId))
            {
                checkin = await InsertCheckinAsync(participant.Id, segment.Id, checkinTime, segment.Order == skipIndex + 1, messageId, Convert.ToUInt32((checkinTime - lastCheckinTime).TotalSeconds), segment);
                checkinSegmentOrder = segment.Order;

                var slackMessage = $"{participant.FullName} ({participant.Bib}) checked into {segment.ToCheckpoint?.Name}, {segment.TotalDistance} miles. ";
                slackMessage += $"{segment.Distance} at {TimeHelpers.CalculatePace(checkin.Elapsed, segment.Distance)} pace";
                
                if (checkin.Confirmed)
                {
                    await _watcherService.NotifyWatchersAsync(participant, segment, checkin);

                    if (segments.Last().Order == checkinSegmentOrder)
                    {
                        await _participantService.SetParticipantStatusAsync(participant.Id, Status.Finished);
                    }
                }
                else
                {
                    slackMessage = $"CHECKIN TO CONFIRM: {slackMessage}";
                    // await _twilioService.SendAdminMessageAsync($"Checkin to confirm for {participant.FullName}.");
                }

                await _slackService.PostMessageAsync(slackMessage, SlackService.Channel.Checkins);

                break;
            }
        }
        // TODO: What to do if no monitor found?

        if (checkinSegmentOrder == -1 && checkin.Id == Guid.Empty)
        {
            // await _twilioService.SendAdminMessageAsync($"Error checking in #{bib} - {participant.FullName}. No monitor recognized.");
            return 0;
        }

        return 1;
    }

    private async Task<Checkin> InsertCheckinAsync(Guid participantId, Guid segmentId, DateTime when, bool confirmed, Guid messageId, uint segmentElapsed, Segment segment)
    {
        var checkin = new Checkin()
        {
            Id = Guid.NewGuid(),
            ParticipantId = participantId,
            SegmentId = segmentId,
            When = when,
            Confirmed = confirmed,
            MessageId = messageId,
            Elapsed = segmentElapsed
        };

        _context.Checkins.Add(checkin);
        await _context.SaveChangesAsync();

        if (confirmed)
        {
            await _leaderService.UpdateLeaderAsync(checkin, segment);
        }

        return checkin;
    }

    public async Task<Checkin> ConfirmCheckinAsync(Guid checkinId, DateTime? when = null, Guid? segmentId = null)
    {
        var checkin = await _context.Checkins.Where(ci => ci.Id == checkinId).FirstAsync();
        var checkins = await GetCheckinsForParticipantAsync(checkin.ParticipantId);
        var participant = await _participantService.GetParticipantAsync(checkin.ParticipantId, true);
        var segment = await _segmentService.GetSegmentAsync(checkin.SegmentId);
        var finishSegment = await _segmentService.GetFinishSegment(participant.RaceId);

        var lastCheckinTime = checkins.Count > 0 ? checkins.OrderByDescending(x => x.When).First().When : participant.Race.Start;

        checkin.Confirmed = true;

        if (segmentId.HasValue)
        {
            checkin.SegmentId = segmentId.Value;
        }

        if (when.HasValue)
        {
            checkin.Elapsed = (uint)(checkin.When - lastCheckinTime).TotalSeconds;
            checkin.When = when.Value;
        }

        if (segment.Id == finishSegment.Id)
        {
            await _participantService.SetParticipantStatusAsync(participant.Id, Status.Finished);
        }
        
        await _context.SaveChangesAsync();

        await _leaderService.UpdateLeaderAsync(checkin, segment);
        await _watcherService.NotifyWatchersAsync(participant, segment, checkin);

        return checkin;
    }

    public async Task<Checkin> GetLastCheckinForParticipant(Guid participantId)
    {
        return await _context.Checkins
            .Where(x => x.ParticipantId == participantId)
            .OrderByDescending(x => x.Segment.Order)
            .Include(x => x.Segment)
            .FirstAsync();
    }

    public async Task<List<Checkin>> GetCheckinsForCheckpointAsync(Guid checkpointId)
    {
        return await _context.Checkins
            .Where(ci => ci.Segment.ToCheckpointId == checkpointId)
            .Where(x => x.Confirmed == true)
            .Include(x => x.Participant)
            .Include(x => x.Message)
            .Include(x => x.Segment)
            .AsNoTracking()
            .ToListAsync();
    }
}