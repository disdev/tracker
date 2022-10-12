using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Services;

public interface IAlertMessageService
{
    Task<List<AlertMessage>> GetCurrentMessagesAsync();
}

public class AlertMessageService : IAlertMessageService
{
    private readonly TrackerContext _context;

    public AlertMessageService(TrackerContext context)
    {
        _context = context;
    }

    public async Task<List<AlertMessage>> GetCurrentMessagesAsync()
    {
        return await _context.AlertMessages
            .Where(x => (x.Start == null || x.Start <= DateTime.UtcNow) && (x.End == null || x.End >= DateTime.UtcNow))
            .ToListAsync();
    }
}