
namespace usbprison;

public class PrisonLog()
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string DeviceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
}