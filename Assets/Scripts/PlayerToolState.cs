public enum ActiveTool { None, Plow, Seed, Tree, Animal, Fire }

public class PlayerToolState
{
    public ActiveTool activeTool = ActiveTool.None;

    public bool plowActive => activeTool == ActiveTool.Plow;
    public bool hasSeed    => activeTool == ActiveTool.Seed;
    public bool hasTree    => activeTool == ActiveTool.Tree;
    public bool hasAnimal  => activeTool == ActiveTool.Animal;
    public bool fireTool   => activeTool == ActiveTool.Fire;

    public void TogglePlow() { activeTool = plowActive ? ActiveTool.None : ActiveTool.Plow; }
    public void ToggleFire() { activeTool = fireTool   ? ActiveTool.None : ActiveTool.Fire; }
    public void SetSeed()    { activeTool = ActiveTool.Seed; }
    public void SetTree()    { activeTool = ActiveTool.Tree; }
    public void SetAnimal()  { activeTool = ActiveTool.Animal; }
    public void Clear()      { activeTool = ActiveTool.None; }
}
