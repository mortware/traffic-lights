using TrafficLights.Models;

namespace TrafficLights.Providers;

public sealed class TrafficLightStatusProvider
{
    private Light SouthToNorth { get; } = new("s_n");

    private Light EastToWest { get; } = new("e_w");

    private Light NorthToSouth { get; } = new("n_s");

    private Light WestToEast { get; } = new("w_e");

    private Light SouthToEast { get; } = new("s_e");
    
    public IEnumerable<Light> All => new[] { NorthToSouth, SouthToNorth, EastToWest, WestToEast, SouthToEast };

    public static TrafficLightStatusProvider Build(TrafficLightFlow current, TrafficLightFlow next, bool isTransitioning)
    {
        return new TrafficLightStatusProvider
        {
            NorthToSouth = { State = GetState(current.NorthToSouthActive, next.NorthToSouthActive, isTransitioning) },
            SouthToNorth = { State = GetState(current.SouthToNorthActive, next.SouthToNorthActive, isTransitioning) },
            EastToWest = { State = GetState(current.WestToEastActive, next.WestToEastActive, isTransitioning) },
            WestToEast = { State = GetState(current.EastToWestActive, next.EastToWestActive, isTransitioning) },
            SouthToEast = { State = GetState(current.SouthToEastActive, next.SouthToEastActive, isTransitioning) }
        };
    }

    private static TrafficLightState GetState(bool isCurrentlyActive, bool isNextActive, bool isTransitioning)
    {
        if (!isTransitioning)
        {
            return isCurrentlyActive
                ? TrafficLightState.Green
                : TrafficLightState.Red;
        }

        return isCurrentlyActive switch
        {
            true when isNextActive => TrafficLightState.Green,
            true when !isNextActive => TrafficLightState.Amber,
            _ => TrafficLightState.Red
        };
    }
}