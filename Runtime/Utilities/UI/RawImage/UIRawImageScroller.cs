using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Utilities.UI
{
    public class UIRawImageScroller : MonoBehaviour
    {
        [SerializeField] RawImage _image;
        [SerializeField] float _x, _y;

        void Update()
        {
            _image.uvRect = new Rect(_image.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _image.uvRect.size);
        }

        void OnValidate()
        {
            if (_image == null) _image = GetComponent<RawImage>();
        }
    }
}
