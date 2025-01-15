using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class ChatHistory : BaseEntity
{
    public string ConservationName { get; set; }
    public int RoomNo { get; set; }
    public Guid? UserId { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<ChatMessage> ChatMessages { get; set; }
}
