using System;


namespace ABC.Infrastructure.Context.Location
{
    public class Detection
    {
        public DateTime DateTime { get; set; }
        public string TagId { get; set; }
        public string HostName { get; set; }
        public int Channel { get; set; }
        public float Amplitude { get; set; }
        public float ConfidenceLevel { get; set; }
        public MovingStatus MovingStatus { get; set; }
        public BatteryStatus BatteryStatus { get; set; }
        public ButtonState ButtonAState { get; set; }
        public ButtonState ButtonBState { get; set; }
        public ButtonState ButtonCState { get; set; }
        public ButtonState ButtonDState { get; set; }
        public bool SelectedField { get; set; }
    }
}