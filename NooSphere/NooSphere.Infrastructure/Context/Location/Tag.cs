namespace ABC.Infrastructure.Context.Location
{
    public class Tag
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public ButtonState ButtonA { get; set; }
        public ButtonState ButtonB { get; set; }
        public ButtonState ButtonC { get; set; }
        public ButtonState ButtonD { get; set; }
        public MovingStatus MovingStatus { get; set; }
        public BatteryStatus BatteryStatus { get; set; }
        public Detector Detector { get; set; }
    }
}