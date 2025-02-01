using System;

namespace Lemon.Toolkit.Models;

public class SourceWordModel
{
    public int Id { get; set; }
    public string Word { get; set; }
    public int ItemType { get; set; }
    public DateTime AddTime { get; set; }
    public int Rating { get; set; }
    public string CategoryTag { get; set; }
}