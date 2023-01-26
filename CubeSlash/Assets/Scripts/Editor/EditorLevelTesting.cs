using UnityEngine;
using UnityEditor;

public static class EditorLevelTesting
{
    [MenuItem("Game/Testing/Enable testing", false, 1)]
    public static void EnableTesting()
    {
        SetTestingEnabled(true);
    }

    [MenuItem("Game/Testing/Disable testing", false, 1)]
    public static void DisableTesting()
    {
        SetTestingEnabled(false);
    }

    private static void SetTestingEnabled(bool enabled)
    {
        var db = LevelDatabase.LoadAsset();
        db.testing_enabled = enabled;
        var level = db.test_level;
        Selection.activeObject = level;
    }
}