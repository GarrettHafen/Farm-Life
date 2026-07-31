using UnityEngine;

// Handles green/red placement preview rendering.
// Attach this to the same GameObject as PlayerInteraction — no scene wiring needed.
[RequireComponent(typeof(PlayerInteraction))]
public class PreviewRenderer : MonoBehaviour
{
    private PlayerInteraction player;

    private void Awake()
    {
        player = GetComponent<PlayerInteraction>();
    }

    private void LateUpdate()
    {
        if (MenuController.instance.toolState.plowActive)
        {
            GameObject preview = MenuController.instance.preview1x1;
            if (!MenuController.instance.GetpreviewPlacementLocation())
            {
                MenuController.instance.ActivatePreview(preview);
                MenuController.instance.SetPreviewCells(4);
            }

            Sprite sprite = MenuController.instance.previewObstructed
                ? GameHandler.instance.previewList[0].previewRedSprite
                : GameHandler.instance.previewList[0].previewGreenSprite;
            MenuController.instance.SetPreviewColor(sprite, preview);
        }

        if (MarketController.instance.marketState == MarketState.Tree && MenuController.instance.toolState.hasTree)
        {
            Tree tree = player.GetTree();
            string treePreview = string.IsNullOrEmpty(tree.asset.preview) ? "2x2" : tree.asset.preview;
            MenuController.instance.SetPreviewCells(TileSelector.PreviewSizeToCells(treePreview));
            GameObject preview = GetPreviewContainer(treePreview);
            int previewPosition = GetPreviewPosition(treePreview);
            if (!MenuController.instance.GetpreviewPlacementLocation())
            {
                MenuController.instance.ActivatePreview(preview);
                MenuController.instance.previewObstructed = false;
            }
            Sprite sprite = MenuController.instance.previewObstructed
                ? GameHandler.instance.previewList[previewPosition].previewRedSprite
                : GameHandler.instance.previewList[previewPosition].previewGreenSprite;
            MenuController.instance.SetPreviewColor(sprite, preview);
        }

        if (MarketController.instance.marketState == MarketState.Animal && MenuController.instance.toolState.hasAnimal)
        {
            Animal animal = player.GetAnimal();
            MenuController.instance.SetPreviewCells(TileSelector.PreviewSizeToCells(animal.asset.preview));
            GameObject preview = GetPreviewContainer(animal.asset.preview);
            int previewPosition = GetPreviewPosition(animal.asset.preview);
            if (!MenuController.instance.GetpreviewPlacementLocation())
            {
                MenuController.instance.ActivatePreview(preview);
                MenuController.instance.previewObstructed = false;
            }
            Sprite sprite = MenuController.instance.previewObstructed
                ? GameHandler.instance.previewList[previewPosition].previewRedSprite
                : GameHandler.instance.previewList[previewPosition].previewGreenSprite;
            MenuController.instance.SetPreviewColor(sprite, preview);
        }

        if (MenuController.instance.toolState.hasDecoration)
        {
            DecorationAsset decoration = player.GetDecoration();
            if (decoration != null)
            {
                MenuController.instance.SetPreviewCells(TileSelector.PreviewSizeToCells(decoration.preview));

                // Full-cell decorations (fences use "4x4") reuse Plow's dedicated whole-cell
                // preview square instead of the generic previewContainerList/previewList "4x4"
                // slot — nothing else has ever driven a real 4x4 footprint through that path,
                // so it was never actually wired up, unlike Plow's known-working preview1x1.
                bool fullCell = decoration.preview == "4x4";
                GameObject preview = fullCell ? MenuController.instance.preview1x1 : GetPreviewContainer(decoration.preview);
                int previewPosition = fullCell ? 0 : GetPreviewPosition(decoration.preview);

                if (!MenuController.instance.GetpreviewPlacementLocation())
                {
                    MenuController.instance.ActivatePreview(preview);
                    MenuController.instance.previewObstructed = false;
                }
                Sprite sprite = MenuController.instance.previewObstructed
                    ? GameHandler.instance.previewList[previewPosition].previewRedSprite
                    : GameHandler.instance.previewList[previewPosition].previewGreenSprite;
                MenuController.instance.SetPreviewColor(sprite, preview);
            }
        }
    }

    private GameObject GetPreviewContainer(string intendedPreview)
    {
        switch (intendedPreview)
        {
            case "1x1": return GameHandler.instance.previewContainerList[0];
            case "4x4": return GameHandler.instance.previewContainerList[1];
            case "2x2": return GameHandler.instance.previewContainerList[6];
        }
        return null;
    }

    private int GetPreviewPosition(string intendedPreview)
    {
        switch (intendedPreview)
        {
            case "1x1": return 0;
            case "4x4": return 8;
            case "2x2": return 5;
        }
        Debug.Log("error in preview position");
        return 0;
    }
}
