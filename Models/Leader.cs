namespace Tracker.Models;

public class Leader
{
    public Guid Id { get; set; }
    public Guid  ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public Guid? CheckpointId { get; set; }
    public Checkpoint? Checkpoint { get; set; }
    public Guid? SegmentId { get; set; }
    public Segment? Segment { get; set; }
    public Guid? CheckinId { get; set; }
    public Checkin? Checkin { get; set; }
    public int OverallTime { get; set; }
    public int OverallPace { get; set; }
}