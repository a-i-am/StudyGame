using UnityEngine;
using UnityEngine.UIElements;
using System.Text;

public partial class StreamExperienceView : VisualElement
{
    private StringBuilder _textBuffer = new StringBuilder();
    private bool _isDirty = false;

    public void AppendText(string chunk)
    {
        _textBuffer.Append(chunk);
        _isDirty = true;
    }

    public void OnUpdate()
    {
        if (_isDirty)
        {
            // Assuming Label_StreamingText is auto-generated
            // Label_StreamingText.text = _textBuffer.ToString();
            _isDirty = false;
        }
    }
}
