using System.Threading.Channels;
using CommunityToolkit.HighPerformance;

namespace VoiceMeeter.NET.Configuration;

public class Eq
{
    private readonly Bus _bus;
    private readonly EqCell[,] _cells = new EqCell[8,6];
    private EqCell[] this[int channel] => this._cells.GetRow(channel).ToArray();

    public string ResourceType => nameof(Eq);

    public Eq(ChangeTracker changeTracker, Bus bus)
    {
        this._bus = bus;

        for (var i = 0; i < 8; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                this._cells[i, j] = new EqCell(changeTracker, this, i, j);
            }
        }
    }

    public EqCell GetCell(int channel, int cellId)
    {
        return this._cells[channel, cellId];
    }
    
    
    internal string GetFullParamName(string paramName)
    {
        return this._bus.GetFullParamName($"EQ.{paramName}");
    }
}