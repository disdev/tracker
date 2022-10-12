using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracker.Data;
using Tracker.Utilities;

namespace Tracker.Models;

public class DbInitializer
{
    private readonly TrackerContext _context;
    
    public DbInitializer(TrackerContext context)
    {
        _context = context;
    }

    public void Initialize(bool addParticipants = false, bool addCheckins = false)
    {
        if (_context.Races.Any())
        {
            EmptyDatabase();
            //return;
        }

        var races = AddRaces();
        var checkpoints = AddCheckpoints();
        var segments = AddSegments(races, checkpoints);
        var monitors = AddMonitors(checkpoints);

        if (addParticipants)
        {
            var participants = AddParticipants(races);

            if (addCheckins)
            {
                AddCheckins(races, segments, participants, true, 6);
                //AddCheckins(races, segments, participants, false);
            }
        }
    }

    private List<Race> AddRaces()
    {
        var e1 = new RaceEvent()
        {
            Id = Guid.NewGuid(),
            Name = "LOVIT",
            Location = "Lake Ouachita",
            Start = new DateTime(2022, 2, 25, 17, 0, 0).ToUniversalTime(),
            Current = true
        };
        _context.RaceEvents.Add(e1);
        _context.SaveChanges();

        var r1 = new Race()
        {
            Id = Guid.NewGuid(),
            Name = "100 Mile",
            Code = "100M",
            Distance = 100.0F,
            Start = new DateTime(2022, 2, 25, 23, 0, 0),
            End = new DateTime(2022, 2, 27, 9, 0, 0),
            UltraSignupUrl = "https://ultrasignup.com/entrants_event.aspx?did=76372",
            RaceEvent = e1,
            Segments = new List<Segment>(),
            Participants = new List<Participant>(),
            Active = true
        };

        var r2 = new Race()
        {
            Id = Guid.NewGuid(),
            Name = "100 Kilometer",
            Code = "100K",
            Distance = 100.0F,
            Start = new DateTime(2022, 2, 26, 12, 0, 0),
            End = new DateTime(2022, 2, 27, 9, 0, 0),
            UltraSignupUrl = "https://ultrasignup.com/entrants_event.aspx?did=76373",
            RaceEvent = e1,
            Segments = new List<Segment>(),
            Participants = new List<Participant>(),
            Active = true
        };

        var races = new List<Race>() { r1, r2 };
        _context.Races.AddRange(races);
        _context.SaveChanges();

        return races;
    }

    private List<Checkpoint> AddCheckpoints()
    {
        var c1 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "East Cove Pavilion",
            GeoJson = @"",
            Number = 0,
            Code = ""
        };

        var c2 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Hickory Nut Mountain",
            GeoJson = @"",
            Number = 1,
            Code = ""
        };

        var c3 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Joplin Road Crossing",
            GeoJson = @"",
            Number = 2,
            Code = ""
        };

        var c4 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Tompkins Bend",
            GeoJson = @"",
            Number = 3,
            Code = ""
        };

        var c5 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "ADA",
            GeoJson = @"",
            Number = 4,
            Code = ""
        };

        var c6 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Forest Road 47A",
            GeoJson = @"",
            Number = 5,
            Code = ""
        };

        var c7 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Charlton",
            GeoJson = @"",
            Number = 6,
            Code = ""
        };

        var c8 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Crystal Springs",
            GeoJson = @"",
            Number = 7,
            Code = ""
        };

        var c9 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Bear Creek",
            GeoJson = @"",
            Number = 8,
            Code = ""
        };

        var c10 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Brady Mountain Road",
            GeoJson = @"",
            Number = 9,
            Code = ""
        };

        var c11 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Spillway ",
            GeoJson = @"",
            Number = 10,
            Code = ""
        };

        var c12 = new Checkpoint()
        {
            Id = Guid.NewGuid(),
            Name = "Avery Rec Area",
            GeoJson = @"",
            Number = 11,
            Code = ""
        };

        var checkpoints = new List<Checkpoint> {
    c1, c2, c3, c4, c5, c6, c7, c8, c9, c10,
            c11, c12
};
        _context.Checkpoints.AddRange(checkpoints);
        _context.SaveChanges();
        return checkpoints;
    }

    private List<Segment> AddSegments(List<Race> races, List<Checkpoint> checkpoints)
    {
        var s1 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Start to Hickory Nut Mountain",
            GeoJson = @"segment-1.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 0),
            ToCheckpoint = checkpoints.First(x => x.Number == 1),
            Order = 1,
            Distance = 4,
            TotalDistance = 4,
            Race = races.First(x => x.Code == "100M")
        };

        var s2 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Hickory Nut Mountain to Joplin Road Crossing",
            GeoJson = @"segment-2.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 1),
            ToCheckpoint = checkpoints.First(x => x.Number == 2),
            Order = 2,
            Distance = 5,
            TotalDistance = 9,
            Race = races.First(x => x.Code == "100M")
        };

        var s3 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Joplin Road Crossing to Tompkins Bend",
            GeoJson = @"segment-3.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 2),
            ToCheckpoint = checkpoints.First(x => x.Number == 3),
            Order = 3,
            Distance = 4,
            TotalDistance = 13,
            Race = races.First(x => x.Code == "100M")
        };

        var s4 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Tompkins Bend to ADA",
            GeoJson = @"segment-4.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 3),
            ToCheckpoint = checkpoints.First(x => x.Number == 4),
            Order = 4,
            Distance = 6,
            TotalDistance = 19,
            Race = races.First(x => x.Code == "100M")
        };

        var s5 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "ADA to Tompkins Bend",
            GeoJson = @"segment-5.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 4),
            ToCheckpoint = checkpoints.First(x => x.Number == 3),
            Order = 5,
            Distance = 6,
            TotalDistance = 25,
            Race = races.First(x => x.Code == "100M")
        };

        var s6 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Tompkins Bend to Joplin Road Crossing",
            GeoJson = @"segment-6.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 3),
            ToCheckpoint = checkpoints.First(x => x.Number == 2),
            Order = 6,
            Distance = 5,
            TotalDistance = 30,
            Race = races.First(x => x.Code == "100M")
        };

        var s7 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Joplin Road Crossing to Hickory Nut Mountain",
            GeoJson = @"segment-7.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 2),
            ToCheckpoint = checkpoints.First(x => x.Number == 1),
            Order = 7,
            Distance = 4,
            TotalDistance = 34,
            Race = races.First(x => x.Code == "100M")
        };

        var s8 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Hickory Nut Mountain to Forest Road 47A",
            GeoJson = @"segment-8.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 1),
            ToCheckpoint = checkpoints.First(x => x.Number == 5),
            Order = 8,
            Distance = 4,
            TotalDistance = 38,
            Race = races.First(x => x.Code == "100M")
        };

        var s9 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Forest Road 47A to Charlton",
            GeoJson = @"segment-9.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 5),
            ToCheckpoint = checkpoints.First(x => x.Number == 6),
            Order = 9,
            Distance = 4,
            TotalDistance = 42,
            Race = races.First(x => x.Code == "100M")
        };

        var s10 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Charlton to Crystal Springs",
            GeoJson = @"segment-10.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 6),
            ToCheckpoint = checkpoints.First(x => x.Number == 7),
            Order = 10,
            Distance = 5,
            TotalDistance = 47,
            Race = races.First(x => x.Code == "100M")
        };

        var s11 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Crystal Springs to Bear Creek",
            GeoJson = @"segment-11.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 7),
            ToCheckpoint = checkpoints.First(x => x.Number == 8),
            Order = 11,
            Distance = 3,
            TotalDistance = 50,
            Race = races.First(x => x.Code == "100M")
        };

        var s12 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Bear Creek to Brady Mountain Road",
            GeoJson = @"segment-12.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 8),
            ToCheckpoint = checkpoints.First(x => x.Number == 9),
            Order = 12,
            Distance = 8,
            TotalDistance = 58,
            Race = races.First(x => x.Code == "100M")
        };

        var s13 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Brady Mountain Road to Spillway",
            GeoJson = @"segment-13.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 9),
            ToCheckpoint = checkpoints.First(x => x.Number == 10),
            Order = 13,
            Distance = 4,
            TotalDistance = 62,
            Race = races.First(x => x.Code == "100M")
        };

        var s14 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Spillway to Avery Rec Area",
            GeoJson = @"segment-14.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 10),
            ToCheckpoint = checkpoints.First(x => x.Number == 11),
            Order = 14,
            Distance = 3,
            TotalDistance = 65,
            Race = races.First(x => x.Code == "100M")
        };

        var s15 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Avery Rec Area to Spillway",
            GeoJson = @"segment-15.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 11),
            ToCheckpoint = checkpoints.First(x => x.Number == 10),
            Order = 15,
            Distance = 3,
            TotalDistance = 68,
            Race = races.First(x => x.Code == "100M")
        };

        var s16 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Spillway to Brady Mountain Road",
            GeoJson = @"segment-16.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 10),
            ToCheckpoint = checkpoints.First(x => x.Number == 9),
            Order = 16,
            Distance = 4,
            TotalDistance = 72,
            Race = races.First(x => x.Code == "100M")
        };

        var s17 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Brady Mountain Road to Bear Creek",
            GeoJson = @"segment-17.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 9),
            ToCheckpoint = checkpoints.First(x => x.Number == 8),
            Order = 17,
            Distance = 8,
            TotalDistance = 80,
            Race = races.First(x => x.Code == "100M")
        };

        var s18 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Bear Creek to Crystal Springs",
            GeoJson = @"segment-18.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 8),
            ToCheckpoint = checkpoints.First(x => x.Number == 7),
            Order = 18,
            Distance = 3,
            TotalDistance = 83,
            Race = races.First(x => x.Code == "100M")
        };

        var s19 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Crystal Springs to Charlton",
            GeoJson = @"segment-19.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 7),
            ToCheckpoint = checkpoints.First(x => x.Number == 6),
            Order = 19,
            Distance = 4,
            TotalDistance = 87,
            Race = races.First(x => x.Code == "100M")
        };

        var s20 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Charlton to Forest Road 47A",
            GeoJson = @"segment-20.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 6),
            ToCheckpoint = checkpoints.First(x => x.Number == 5),
            Order = 20,
            Distance = 5,
            TotalDistance = 92,
            Race = races.First(x => x.Code == "100M")
        };

        var s21 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Forest Road 47A to Hickory Nut Mountain",
            GeoJson = @"segment-21.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 5),
            ToCheckpoint = checkpoints.First(x => x.Number == 1),
            Order = 21,
            Distance = 4,
            TotalDistance = 96,
            Race = races.First(x => x.Code == "100M")
        };

        var s22 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Hickory Nut Mountain to Finish",
            GeoJson = @"segment-22.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 1),
            ToCheckpoint = checkpoints.First(x => x.Number == 0),
            Order = 22,
            Distance = 4,
            TotalDistance = 100,
            Race = races.First(x => x.Code == "100M")
        };



        var s23 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Start to Hickory Nut Mountain",
            GeoJson = @"segment-1.json",
            FromCheckpoint = checkpoints.First(x => x.Number == 0),
            ToCheckpoint = checkpoints.First(x => x.Number == 1),
            Order = 1,
            Distance = 4,
            TotalDistance = 4,
            Race = races.First(x => x.Code == "100K")
        };
        var s24 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Hickory Nut Mountain to Forest Road 47A",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 1),
            ToCheckpoint = checkpoints.First(x => x.Number == 5),
            Order = 2,
            Distance = 3.5,
            TotalDistance = 7.5,
            Race = races.First(x => x.Code == "100K")
        };

        var s25 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Forest Road 47A to Crystal Springs",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 5),
            ToCheckpoint = checkpoints.First(x => x.Number == 7),
            Order = 3,
            Distance = 4.5,
            TotalDistance = 12,
            Race = races.First(x => x.Code == "100K")
        };

        var s26 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Crystal Springs to Bear Creek",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 7),
            ToCheckpoint = checkpoints.First(x => x.Number == 8),
            Order = 4,
            Distance = 3.5,
            TotalDistance = 15.5,
            Race = races.First(x => x.Code == "100K")
        };

        var s27 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Bear Creek to Brady Mountain Road",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 8),
            ToCheckpoint = checkpoints.First(x => x.Number == 9),
            Order = 5,
            Distance = 7.5,
            TotalDistance = 23,
            Race = races.First(x => x.Code == "100K")
        };

        var s28 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Brady Mountain Road to Spillway",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 9),
            ToCheckpoint = checkpoints.First(x => x.Number == 10),
            Order = 6,
            Distance = 4,
            TotalDistance = 27,
            Race = races.First(x => x.Code == "100K")
        };

        var s29 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Spillway to Avery Rec Area",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 10),
            ToCheckpoint = checkpoints.First(x => x.Number == 11),
            Order = 7,
            Distance = 3,
            TotalDistance = 30,
            Race = races.First(x => x.Code == "100K")
        };

        var s30 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Avery Rec Area to Spillway",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 11),
            ToCheckpoint = checkpoints.First(x => x.Number == 10),
            Order = 8,
            Distance = 3,
            TotalDistance = 33,
            Race = races.First(x => x.Code == "100K")
        };

        var s31 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Spillway to Brady Mountain Road",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 10),
            ToCheckpoint = checkpoints.First(x => x.Number == 9),
            Order = 9,
            Distance = 3,
            TotalDistance = 37,
            Race = races.First(x => x.Code == "100K")
        };

        var s32 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Brady Mountain Road to Bear Creek",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 9),
            ToCheckpoint = checkpoints.First(x => x.Number == 8),
            Order = 10,
            Distance = 7.5,
            TotalDistance = 44.5,
            Race = races.First(x => x.Code == "100K")
        };

        var s33 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Bear Creek to Crystal Springs",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 8),
            ToCheckpoint = checkpoints.First(x => x.Number == 7),
            Order = 11,
            Distance = 3.5,
            TotalDistance = 48,
            Race = races.First(x => x.Code == "100K")
        };

        var s34 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Crystal Springs to Forest Road 47A",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 7),
            ToCheckpoint = checkpoints.First(x => x.Number == 5),
            Order = 12,
            Distance = 4.5,
            TotalDistance = 52.5,
            Race = races.First(x => x.Code == "100K")
        };

        var s35 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Forest Road 47A to Hickory Nut Mountain",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 5),
            ToCheckpoint = checkpoints.First(x => x.Number == 1),
            Order = 13,
            Distance = 3.5,
            TotalDistance = 56,
            Race = races.First(x => x.Code == "100K")
        };

        var s36 = new Segment()
        {
            Id = Guid.NewGuid(),
            Name = "Hickory Nut Mountain to Finish",
            GeoJson = @"",
            FromCheckpoint = checkpoints.First(x => x.Number == 1),
            ToCheckpoint = checkpoints.First(x => x.Number == 0),
            Order = 14,
            Distance = 4,
            TotalDistance = 60,
            Race = races.First(x => x.Code == "100K")
        };

        var r1Segments = new List<Segment>()
        {
            s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16, s17, s18, s19, s20, s21, s22
        };

        var r2Segments = new List<Segment>()
        {
            s23, s24, s25, s26, s27, s28, s29, s30, s31, s32, s33, s34, s35, s36
        };

        _context.Segments.AddRange(r1Segments);
        _context.Segments.AddRange(r2Segments);
        _context.SaveChanges();

        var segments = new List<Segment>();
        segments.AddRange(r1Segments);
        segments.AddRange(r2Segments);

        return segments;
    }

    private List<Monitor> AddMonitors(List<Checkpoint> checkpoints)
    {
        var monitors = new List<Monitor>();

        foreach (var checkpoint in _context.Checkpoints)
        {
            monitors.Add(new Monitor()
            {
                Checkpoint = checkpoint,
                Name = "John Doe",
                PhoneNumber = "+1111111111",
                Active = true
            });
        }

        _context.Monitors.AddRange(monitors);
        _context.SaveChanges();

        return monitors;
    }

    private List<Participant> AddParticipants(List<Race> races)
    {
        var participants = new List<Participant>();
        var leaders = new List<Leader>();

        foreach (var race in races)
        {
            for (int i = 1; i <= 100; i++)
            {
                var bibStart = (race.Code == "100M") ? 100 : 1000;
                var participant = new Participant()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Person",
                    LastName = $"{i}",
                    Bib = $"{bibStart + i}",
                    City = "Anywhere",
                    Region = "US",
                    Age = "1-100",
                    Gender = Gender.Male,
                    RaceId = race.Id,
                    Status = Status.Registered,
                    PictureUrl = "empty",
                    UltraSignupEmail = "email"
                };

                participants.Add(participant);

                var leader = new Leader()
                {
                    ParticipantId = participant.Id
                };

                leaders.Add(leader);
            }
        }

        _context.Participants.AddRange(participants);
        _context.Leaders.AddRange(leaders);
        _context.SaveChanges();

        return participants;
    }

    private void AddCheckins(List<Race> races, List<Segment> segments, List<Participant> participants, bool limit = false, int hourLimit = 6) {
        var random = new Random();

        foreach (var race in races)
        {
            var raceParticipants = participants.Where(x => x.RaceId == race.Id).ToList();
            var raceSegments = segments.Where(x => x.RaceId == race.Id).OrderBy(x => x.Order).ToList();

            foreach (var participant in raceParticipants)
            {
                var messages = new List<Message>();
                var checkins = new List<Checkin>();
                var quality = random.Next(1, 10);
                DateTime lastCheckinTime = race.Start;

                participant.Status = Status.Started;

                foreach (var segment in raceSegments)
                {
                    DateTime when = CalculateWhen(segment.Distance, quality, lastCheckinTime);
                    uint elapsed = (uint)(when - lastCheckinTime).TotalSeconds;

                    if (limit == false || when <= race.Start.AddHours(hourLimit))
                    {
                        var message = new Message()
                        {
                            Id = Guid.NewGuid(),
                            From = "+1111111111",
                            Body = participant.Bib,
                            FromCity = "NONE",
                            FromState = "NONE",
                            FromZip = "NON",
                            FromCountry = "NONE",
                            Received = when
                        };

                        messages.Add(message);

                        var checkin = new Checkin()
                        {
                            Id = Guid.NewGuid(),
                            When = when,
                            ParticipantId = participant.Id,
                            SegmentId = segment.Id,
                            Confirmed = true,
                            MessageId = message.Id,
                            Elapsed = elapsed
                        };

                        checkins.Add(checkin);

                        if (segment.Id == raceSegments.Last().Id)
                        {
                            participant.Status = Status.Finished;
                            var leader = _context.Leaders.Where(x => x.ParticipantId == participant.Id).Single();
                            leader.CheckinId = checkin.Id;
                            leader.CheckpointId = segment.ToCheckpointId;
                            leader.SegmentId = segment.Id;
                            leader.OverallTime = (int)(when - race.Start).TotalSeconds;
                            leader.OverallPace = (int)TimeHelpers.CalculatePaceInSeconds(race.Start, when, segment.TotalDistance);
                            _context.Leaders.Update(leader);
                        }

                        lastCheckinTime = when;
                    }
                    else
                    {
                        break;
                    }
                }

                _context.Messages.AddRange(messages);
                _context.Checkins.AddRange(checkins);
                _context.SaveChanges();
            }
        }
    }

    private DateTime CalculateWhen(double distance, int quality, DateTime last)
    {
        Random rnd = new Random();

        int mins = rnd.Next(8 + quality, 14 + quality);
        int secs = rnd.Next(0, 59);

        int pace = mins * 60 + secs;
        double elapsed = pace * distance;

        return last.AddSeconds(elapsed);
    }

    public void EmptyDatabase()
    {
        _context.Participants.RemoveRange(_context.Participants);
        _context.Leaders.RemoveRange(_context.Leaders);
        _context.Watchers.RemoveRange(_context.Watchers);
        _context.Checkpoints.RemoveRange(_context.Checkpoints);
        _context.Segments.RemoveRange(_context.Segments);
        _context.Monitors.RemoveRange(_context.Monitors);
        _context.Checkins.RemoveRange(_context.Checkins);
        _context.Races.RemoveRange(_context.Races);
        _context.RaceEvents.RemoveRange(_context.RaceEvents);
        _context.SaveChanges();
    }
}