using SketchEngine.Core.UI;
using UnityEngine;

namespace SketchEngine.Utilities.UI
{
    public class UIButtonSpawnObject : UIButtonBase
    {
        [SerializeField] GameObject _prefab;

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            if (_prefab == null) return;
            Instantiate(_prefab);
        }
    }
}
