using System;

namespace XWTFunctions.Messages;

public class PlayerRegistrationMessage
{
    public Guid TournamentId { get; set; }
    public Guid PlayerId { get; set; }
}