namespace CustomPress.Models;

public class RepeatingBlock
{
    public int StartIndex  { get; set; }
    public int BlockLength { get; set; }
    public int RepeatCount { get; set; }
    public List<double> Block { get; set; } = [];
}
