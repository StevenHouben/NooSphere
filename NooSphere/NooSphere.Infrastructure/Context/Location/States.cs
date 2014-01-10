namespace ABC.Infrastructure.Context.Location
{
    public enum ButtonState
    {
        Pressed = 1,
        NotPressed = 0,
        Undefined = -1
    }

    public enum OperationStatus
    {
        Offline,
        Online
    }

    public enum MovingStatus
    {
        Moving = 1,
        NonMoving,
        Undefined = -1
    }

    public enum BatteryStatus
    {
        Ok = 0,
        Undefined = -1,
        Low
    }
}