using ImGuiNET;
using UImGui;
using UnityEngine;

public partial class DemoSelfAvoidingWalkEx : MonoBehaviour
{
    private void OnEnable()
    {
        UImGuiUtility.Layout += OnLayout;
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
    }

    private void OnLayout(UImGui.UImGui uImGui)
    {
        if (ImGui.Begin("Self-Avoiding walk with backtracking"))
        {
            ImGui.Text($"Grid size: {ImageGrid.GridSize}");
            ImGui.Text($"Seed: {Seed}");
            ImGui.Text($"Step: {_step}");

            ImGui.End();
        }
    }
}
