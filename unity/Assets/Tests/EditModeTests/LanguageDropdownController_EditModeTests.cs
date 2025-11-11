using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;

/// <summary>
/// EDIT MODE tests for LanguageDropdownController.
/// Expectations:
/// 1) When the LanguageDropdownController is awakened, it should populate the TMP_Dropdown with language options.
/// 2) It should also set the default selected language to "Spanish".
/// 3) Selecting a language should propagate the target code to the WebSocketManager.
/// </summary>
public class LanguageDropdownController_EditModeTests
{
    [SetUp]
    public void SetUp()
    {
        ResetSelection();
        ClearWebSocketInstance();
    }

    [TearDown]
    public void TearDown()
    {
        ClearWebSocketInstance();
        ResetSelection();
    }

    [Test]
    public void Dropdown_Populates_OnAwake()
    {
        var go = new GameObject("LanguageDropdown");
        var dropdown = go.AddComponent<TMP_Dropdown>();
        var status = new GameObject("StatusText").AddComponent<TextMeshProUGUI>();
        var controller = go.AddComponent<LanguageDropdownController>();
        typeof(LanguageDropdownController)
            .GetField("statusText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, status);

        var awakeMethod = typeof(LanguageDropdownController).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        awakeMethod.Invoke(controller, null);

        Assert.IsTrue(dropdown.options.Count > 0, "Expected dropdown to be populated with options.");
        Assert.AreEqual("Spanish", LanguageDropdownController.SelectedLabel, "Expected default language to be Spanish.");

        Object.DestroyImmediate(status.gameObject);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Dropdown_Selection_Updates_WebSocket_TargetLanguage()
    {
        var wsObject = new GameObject("WebSocketManager");
        var manager = wsObject.AddComponent<WebSocketManager>();
        typeof(WebSocketManager)
            .GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
            ?.GetSetMethod(true)
            ?.Invoke(null, new object[] { manager });

        var go = new GameObject("LanguageDropdown");
        var dropdown = go.AddComponent<TMP_Dropdown>();
        var status = new GameObject("StatusText").AddComponent<TextMeshProUGUI>();
        var controller = go.AddComponent<LanguageDropdownController>();
        typeof(LanguageDropdownController)
            .GetField("statusText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, status);

        var awakeMethod = typeof(LanguageDropdownController).GetMethod(
            "Awake",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        awakeMethod.Invoke(controller, null);

        int frenchIndex = dropdown.options.FindIndex(option => option.text == "French");
        Assert.GreaterOrEqual(frenchIndex, 0, "French option should exist in the dropdown.");

        dropdown.value = frenchIndex;
        dropdown.onValueChanged.Invoke(frenchIndex);

        Assert.AreEqual("fr", manager.targetLanguage, "WebSocketManager target language should update to the selected code.");
        Assert.AreEqual("fr", LanguageDropdownController.SelectedCode, "Selected code should reflect dropdown choice.");

        Object.DestroyImmediate(status.gameObject);
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(wsObject);
    }

    private static void ResetSelection()
    {
        typeof(LanguageDropdownController)
            .GetProperty("SelectedLabel", BindingFlags.Static | BindingFlags.Public)
            ?.GetSetMethod(true)
            ?.Invoke(null, new object[] { "Spanish" });

        typeof(LanguageDropdownController)
            .GetProperty("SelectedCode", BindingFlags.Static | BindingFlags.Public)
            ?.GetSetMethod(true)
            ?.Invoke(null, new object[] { "es" });
    }

    private static void ClearWebSocketInstance()
    {
        typeof(WebSocketManager)
            .GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
            ?.GetSetMethod(true)
            ?.Invoke(null, new object[] { null });
    }
}
