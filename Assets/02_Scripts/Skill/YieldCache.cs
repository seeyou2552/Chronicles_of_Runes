using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YieldCache : MonoBehaviour
{
    /// Boxing 발생하지 않게 해주며, 의도치 않게 가비지가 생성되는 것을 방지
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }
        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    private static readonly Dictionary<float, WaitForSeconds> timeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());

    // 코루틴 Yield WaitForSeconds 최적화
    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!timeInterval.TryGetValue(seconds, out WaitForSeconds waitForSeconds))
            timeInterval.Add(seconds, waitForSeconds = new WaitForSeconds(seconds));
        return waitForSeconds;
    }
}
