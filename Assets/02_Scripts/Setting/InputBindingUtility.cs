using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class InputBindingUtility
{
    // 매핑 가능한 키워드 → 이니셜 문자
    private static readonly Dictionary<string, string> specialKeyMap = new Dictionary<string, string>
    {
        // 방향키
        { "Left Arrow",  "←" },
        { "Right Arrow", "→" },
        { "Up Arrow",    "↑" },
        { "Down Arrow",  "↓" },

        // 제어키
        { "Space",       "Sp" },
        { "Enter",       "En" },
        { "Escape",      "Esc" },
        { "Tab",         "Tab" },
        { "Backspace",   "Bk" },
        { "Delete",      "Del" },
        { "Caps Lock",   "Caps" },

        // 조합키
        { "Left Shift",  "Sh" },
        { "Right Shift", "Sh" },
        { "Shift",       "Sh" }, // 예외 대응

        { "Left Ctrl",   "Ctl" },
        { "Right Ctrl",  "Ctl" },
        { "Ctrl",        "Ctl" },

        { "Left Alt",    "Alt" },
        { "Right Alt",   "Alt" },
        { "Alt",         "Alt" },

        // 마우스 버튼
        { "Left Button",   "LMB" },
        { "Right Button",  "RMB" },
        { "Middle Button", "MMB" }
    };
    
    public static string GetInitialForPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "?";

        string readable = InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );

        if (specialKeyMap.TryGetValue(readable, out var symbol))
        {
            return symbol;
        }

        return readable.Length > 0
            ? readable[0].ToString().ToUpper()
            : "?";
    }
}