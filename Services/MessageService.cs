using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Services;

public interface IMessageService
{
    Task<List<Message>> GetMessagesAsync();
    Task<Message> GetMessageAsync(Guid messageId);
    Task<Message> AddMessageAsync(Message message);
    Task<string> HandleMessageAsync(Message message);
}

public class MessageService : IMessageService
{
    private readonly TrackerContext _context;
    private readonly IRaceService _raceService;
    private readonly IMonitorService _monitorService;
    private readonly IWatcherService _watcherService;
    // private readonly ITwilioService _twilioService;
    private readonly ICheckinService _checkinService;
    private readonly SlackService _slackService;

    public MessageService(TrackerContext context, IRaceService raceService, IMonitorService monitorService, IWatcherService watcherService, ICheckinService checkinService, SlackService slackService)
    {
        _context = context;
        _raceService = raceService;
        _monitorService = monitorService;
        _watcherService = watcherService;
        // _twilioService = twilioService;
        _checkinService = checkinService;
        _slackService = slackService;
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        return await _context.Messages.OrderBy(x => x.Received).ToListAsync();
    }

    public async Task<Message> GetMessageAsync(Guid messageId)
    {
        return await _context.Messages.Where(x => x.Id == messageId).FirstAsync();
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        message.Id = Guid.NewGuid();
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await _slackService.PostMessageAsync($"Message: {message.Body}, From: {message.From}", SlackService.Channel.Messages);

        return message;
    }

    public async Task<string> HandleMessageAsync(Message message) 
    {
        var messageParts = message.Body!.Trim().Split(' ');

        if (messageParts[0].Trim().ToUpper(CultureInfo.InvariantCulture) == "START")
        {
            var race = await _raceService.StartRace(message.From, messageParts[1].Trim(), message.Received);
            return $"Started {race.Code}.";
        }
        else if (messageParts[0].Trim().ToUpper(CultureInfo.InvariantCulture) == "SETUP")
        {
            var monitor = await _monitorService.AddMonitor(message.From, Convert.ToInt16(messageParts[1].Trim(), CultureInfo.InvariantCulture));
            await _slackService.PostMessageAsync($"{message.From} is a monitor for {monitor.Checkpoint?.Name}", SlackService.Channel.Monitors);
            return $"You're set up as a monitor for {monitor.Checkpoint?.Name}.";
        }
        else if (messageParts[0].Trim().ToUpper(CultureInfo.InvariantCulture) == "STOP")
        {
            await _watcherService.DisableAllWatchersForPhoneAsync(message.From);
            await _slackService.PostMessageAsync($"{message.From} sent a STOP message.", SlackService.Channel.Messages);
            return $"You will no longer receive race updates. If you'd like to change this, please sign up for updates online again.";
        }
        else if (!message.Body.Replace(" ", "").All(char.IsDigit))
        {
            var isValidMonitor = await _monitorService.IsValidMonitor(message.From);
            var monitors = await _monitorService.GetMonitorsForPhoneNumberAsync(message.From);
            
            // await _twilioService.SendAdminMessageAsync($"Bad message from {message.From.ToString()}. Monitor: {isValidMonitor.ToString()}. Message: {message.Body}.");

            if (isValidMonitor)
            {
                var monitorList = string.Join(",", monitors.Select(x => x.Checkpoint.Name));
                await _slackService.PostMessageAsync($"Monitor {message.From} from {monitorList} sent an unhandled message: {message.Body}", SlackService.Channel.Exceptions);
                return "I only understand race bib numbers. Your message has been forwarded to the race director for review.";
            }
            else
            {
                await _slackService.PostMessageAsync($"{message.From} sent an unhandled message: {message.Body}", SlackService.Channel.Exceptions);
                return $"This is an automated system that handles race updates. We cannot respond personally to incoming messages.";
            }
        }
        else
        {
            var checkinCount = await _checkinService.HandleCheckinsAsync(message);
            var responseText = $"Checked in {checkinCount} runner{(checkinCount > 1 || checkinCount == 0 ? "s" : "")}.";
            if (checkinCount == 0)
            {
                responseText += " An error has occurred. The race director has been notified.";
            }
            return responseText;
        }
    }
}