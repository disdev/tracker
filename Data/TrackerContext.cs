using Microsoft.EntityFrameworkCore;
using Tracker.Models;

namespace Tracker.Data;

public class TrackerContext : DbContext
{
    public TrackerContext (DbContextOptions<TrackerContext> options)
        : base(options)
    {
    }

    public DbSet<RaceEvent> RaceEvents => Set<RaceEvent>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<Checkpoint> Checkpoints => Set<Checkpoint>();
    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Tracker.Models.Monitor> Monitors => Set<Tracker.Models.Monitor>();
    public DbSet<Checkin> Checkins => Set<Checkin>();
    public DbSet<Watcher> Watchers => Set<Watcher>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Leader> Leaders => Set<Leader>();
    public DbSet<AlertMessage> AlertMessages => Set<AlertMessage>();
}