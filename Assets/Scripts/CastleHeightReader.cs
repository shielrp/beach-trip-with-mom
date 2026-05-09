using System.Linq;
using UnityEngine;

public class CastleHeightReader : MonoBehaviour
{
    public float BaseWorldHeight;

    public float GetWorldHeightOfCastle()
    {
        DropObject[] dropObjs = GetComponentsInChildren<DropObject>().Where(o => o.IsSettled()).ToArray();

        float hieghestPoint = BaseWorldHeight;

        foreach (DropObject dropObj in dropObjs)
        {
            float highest = dropObj.GetHighestPoint();

            if (highest > hieghestPoint)
                hieghestPoint = highest;
        }

        return (hieghestPoint - BaseWorldHeight) / 2f;
    }
}
