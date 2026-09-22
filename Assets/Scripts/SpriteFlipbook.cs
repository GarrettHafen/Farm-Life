using UnityEngine;

// Plays a randomly-selected animation variant on loop, then switches to a different
// random variant after a randomized hold duration. Frame/hold timing is driven
// externally via Advance() so it can run from any Update() without owning a MonoBehaviour.
public class SpriteFlipbook
{
    private AnimalAnimVariant[] variants;
    private float frameRate;
    private float minHold;
    private float maxHold;

    private int variantIndex;
    private int frameIndex;
    private float frameTimer;
    private float holdTimer;
    private float holdDuration;

    public void SetVariants(AnimalAnimVariant[] newVariants, float newFrameRate, float newMinHold, float newMaxHold)
    {
        variants = newVariants;
        frameRate = Mathf.Max(0.01f, newFrameRate);
        minHold = newMinHold;
        maxHold = Mathf.Max(newMinHold, newMaxHold);

        variantIndex = (variants != null && variants.Length > 0) ? Random.Range(0, variants.Length) : 0;
        frameIndex = 0;
        frameTimer = 0f;
        holdTimer = 0f;
        holdDuration = Random.Range(minHold, maxHold);
    }

    public Sprite Advance(float deltaTime)
    {
        if (variants == null || variants.Length == 0)
            return null;

        Sprite[] frames = variants[variantIndex].frames;
        if (frames == null || frames.Length == 0)
            return null;

        float frameDuration = 1f / frameRate;
        frameTimer += deltaTime;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        if (variants.Length > 1)
        {
            holdTimer += deltaTime;
            if (holdTimer >= holdDuration)
                PickNewVariant();
        }

        return frames[frameIndex];
    }

    private void PickNewVariant()
    {
        int nextIndex = Random.Range(0, variants.Length - 1);
        if (nextIndex >= variantIndex) nextIndex++; // skip current index so it never repeats immediately

        variantIndex = nextIndex;
        frameIndex = 0;
        frameTimer = 0f;
        holdTimer = 0f;
        holdDuration = Random.Range(minHold, maxHold);
    }
}
