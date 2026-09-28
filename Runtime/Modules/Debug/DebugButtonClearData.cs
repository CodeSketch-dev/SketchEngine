using SketchEngine.Data;
using UnityEngine;

namespace SketchEngine.Debug
{
    public class DebugButtonClearData : DebugButton
    {
        public override void Button_OnClick()
        {
            base.Button_OnClick();

            DataFileHandler.DeleteAllInDevice();

            Application.Quit();
        }
    }
}
