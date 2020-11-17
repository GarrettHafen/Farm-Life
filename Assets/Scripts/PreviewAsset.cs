using UnityEngine;

[CreateAssetMenu(fileName = "New Preview", menuName = "Preview")]
public class PreviewAsset : ScriptableObject
{
    public string previewName; //description of the preview
    public Sprite previewGreenSprite;
    public Sprite previewRedSprite;
}
