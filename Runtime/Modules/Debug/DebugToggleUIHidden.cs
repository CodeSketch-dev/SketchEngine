using SketchEngine.Data;

namespace SketchEngine.Debug
{
    public class DebugToggleUIHidden : DebugToggle
    {
        protected override bool IsOn 
        { 
            get => DataMaster.UIHidden.Value;
            set => DataMaster.UIHidden.Value = value;
        }
    }
}
