using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for the IsaacStageManager to add debug functionalities.
/// </summary>
[CustomEditor(typeof(IsaacStageManager))]
public class IsaacStageManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector elements

        IsaacStageManager stageManager = (IsaacStageManager)target;

        EditorGUILayout.Space(); // Add some spacing
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Open All Doors"))
        {
            if (Application.isPlaying)
            {
                OpenAllDoorsInScene(stageManager);
            }
            else
            {
                EditorUtility.DisplayDialog("Debug Action Notice", "This action can only be performed during Play mode.", "OK");
                Debug.LogWarning("[IsaacStageManagerEditor] 'Open All Doors' can only be used during Play mode.");
            }
        }
    }

    private void OpenAllDoorsInScene(IsaacStageManager manager)
    {
        DoorController[] allDoors = FindObjectsByType<DoorController>(FindObjectsSortMode.None);
        
        if (allDoors.Length == 0)
        {
            Debug.LogWarning("[IsaacStageManagerEditor] No DoorControllers found in the current scene.");
            return;
        }

        int doorsOpened = 0;
        int doorsSkipped = 0;
        foreach (DoorController door in allDoors)
        {
            if (door.state == DoorState.Wall)
            {
                doorsSkipped++;
                continue; // Skip doors that are currently walls
            }

            if (door.state != DoorState.Open) 
            {
                door.SetState(DoorState.Open);
                doorsOpened++;
            }
        }

        if (doorsOpened > 0)
        {
            Debug.Log($"[IsaacStageManagerEditor] Opened {doorsOpened} door(s) via debug button.", manager);
        }
        else if (doorsSkipped == allDoors.Length && allDoors.Length > 0) // All doors were walls
        {
             Debug.Log("[IsaacStageManagerEditor] All found doors are currently in the 'Wall' state and were skipped.", manager);
        }
        else if (allDoors.Length > 0) // No doors were opened, but not all were walls (e.g., already open)
        {
            Debug.Log("[IsaacStageManagerEditor] No doors needed to be opened (already open or are walls).", manager);
        }

        if (doorsSkipped > 0)
        {
            Debug.Log($"[IsaacStageManagerEditor] Skipped {doorsSkipped} door(s) because they were in the 'Wall' state.", manager);
        }
    }
}
