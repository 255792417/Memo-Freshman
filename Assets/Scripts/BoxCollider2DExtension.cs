
using UnityEngine;

public static class BoxCollider2DExtension
{
    public static void CopyFrom(this BoxCollider2D target, BoxCollider2D source)
    {
        target.offset = source.offset;
        target.size = source.size;
        target.isTrigger = source.isTrigger;
        target.sharedMaterial = source.sharedMaterial;
        target.usedByEffector = source.usedByEffector;
        target.enabled = source.enabled;
    }
}
