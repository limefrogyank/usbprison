
using SQLite;

namespace usbprison;

// if we use bit masking, can use bit 0 for plugged in and bit 1 for lockdown state
public enum PrisonStatus
{
    Free = 0,   // NOT plugged in, but NOT lockdown         00
    Home = 1,    // plugged in, but NOT lockdown            01
    Escaped = 2,  // NOT plugged in, but lockdown state     10
    Locked = 3,  // plugged in, but lockdown state          11
}

public class PrisonLog()
{
    [PrimaryKey,AutoIncrement] public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.MinValue;
    public string DeviceId { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public PrisonStatus Status { get; set; } = PrisonStatus.Free;
    public DateTime LockdownStart { get; set; } = DateTime.MinValue;  // bookends the current time, so could be first or last 
    public DateTime LockdownEnd { get; set;} = DateTime.MinValue;     // bookends the current time, so could be first or last 
}