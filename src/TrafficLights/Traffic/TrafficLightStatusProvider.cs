namespace TrafficLights.Traffic;

public sealed class TrafficLightStatusProvider
{
    public Light SouthToNorth { get; } = new(1, "s_n");

    public Light EastToWest { get; } = new(2, "e_w");

    public Light NorthToSouth { get; } = new(3, "n_s");

    public Light WestToEast { get; } = new(4, "w_e");

    public Light SouthToEast { get; } = new(5, "s_e");
    
    public IEnumerable<Light> All => new[] { NorthToSouth, SouthToNorth, EastToWest, WestToEast, SouthToEast };

    public static TrafficLightStatusProvider Build(TrafficLightState current, TrafficLightState next, bool isTransitioning)
    {
        return new TrafficLightStatusProvider
        {
            NorthToSouth = { State = GetState(current.NorthToSouthActive, next.NorthToSouthActive, isTransitioning) },
            SouthToNorth = { State = GetState(current.SouthToNorthActive, next.SouthToNorthActive, isTransitioning) },
            EastToWest = { State = GetState(current.WestToEastActive, next.WestToEastActive, isTransitioning) },
            WestToEast = { State = GetState(current.EastToWestActive, next.EastToWestActive, isTransitioning) },
            SouthToEast = { State = GetState(current.SouthToEastActive, next.SouthToEastActive, isTransitioning) },
        };
    }

    private static State GetState(bool isCurrentlyActive, bool isNextActive, bool isTransitioning)
    {
        if (!isTransitioning)
        {
            return isCurrentlyActive
                ? State.Green
                : State.Red;
        }

        return isCurrentlyActive switch
        {
            true when isNextActive => State.Green,
            true when !isNextActive => State.Amber,
            _ => State.Red
        };
    }
}