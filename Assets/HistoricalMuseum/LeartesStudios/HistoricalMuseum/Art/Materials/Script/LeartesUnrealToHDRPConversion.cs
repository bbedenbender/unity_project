using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class LeartesUnrealToHDRPConversion : EditorWindow
{
    [SerializeField] GameObject targetPrefab;
    GameObject[] prefabsWithoutMaterials;
    
    
    Material[] targetMaterials;
    
    
    [MenuItem("Window/Leartes")]
    public static void ShowWindow()
    {
        GetWindow<LeartesUnrealToHDRPConversion>("Leartes");
    }


     void OnGUI()
    {
        GUILayout.Label("[+] Prefab Replacer [+]", EditorStyles.boldLabel);
        GUILayout.Label("How to use the Prefab Replacer\n\nStep 1: Drag and Drop the prefab that you want to replace\nwith scene objects into the targetPrefab section\n\nStep 2: Select the Scene Objects that you want to replace\nwith, if the desired object contains child objects (like LODs)\nonly select the parent object \n\nStep 3: Hit the Replace Button");
        targetPrefab = (GameObject)EditorGUILayout.ObjectField("PrefabToReplaceWith", targetPrefab, typeof(GameObject), false);

        if (GUILayout.Button("ReplacePrefabs"))
        {
            GameObject[] selection = Selection.gameObjects;

            for (int i = 0; i < selection.Length; i++)
            {
                GameObject selected = selection[i];
                var prefabType = PrefabUtility.GetPrefabType(targetPrefab);
                GameObject newObject;

                if (prefabType == PrefabType.Prefab)
                {
                    newObject = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab);
                }
                else
                {
                    newObject = Instantiate(targetPrefab);
                    newObject.name = targetPrefab.name + i.ToString();
                }
                
                if (newObject == null)
                {
                    Debug.LogError("Error: Could not instantiate the target prefab or the input -targetPrefab- was not a prefab.");
                    break;
                }

                Undo.RegisterCompleteObjectUndo(newObject, "Replace Selected with Prefabs");
                newObject.transform.parent = selected.transform.parent;
                newObject.transform.localPosition = selected.transform.localPosition;
                newObject.transform.localRotation = selected.transform.localRotation;
                newObject.transform.localScale = selected.transform.localScale;
                newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                newObject.name = targetPrefab.name + i.ToString();
                Undo.DestroyObjectImmediate(selected);

            }
        }
        GUILayout.Label("Selection count: " + Selection.objects.Length);

        GUILayout.Label("\n[+] Material Replacer [+]", EditorStyles.boldLabel);

        GUILayout.Label("How to Use the Material Replacer\n\nStep 1: Create a simple cube and turn it into prefab inside the project\n\nStep 2: Delete Cube from Scene and select the Cube prefab\n\nStep 3: Select all the new materials that you have created and\n\n attach them to the cube and\n\ncache the materials with StoreNewMaterials Button\n\nStep 4: Select all of your prefabs from the project folder,\n\nClick the Replace Materials Button, and Save your Scene for the changes to apply");


        if (GUILayout.Button("StoreNewMaterials"))
        {
            GameObject[] selected = Selection.gameObjects;
            
            targetMaterials = selected[0].GetComponent<MeshRenderer>().sharedMaterials;

            //Giving Feedback to User
            Debug.Log("Stored the following materials to cache: ");
            foreach(Material material in targetMaterials)
            {
                Debug.Log(material.name);
                //material.name = material.name.Insert(1, "I").ToString(); //Adding I to string so that it changes to material instance namespace of unreal engine //Commented this part out later on and added to the second section
                //mat.name = mat.name.Remove(1, 0).ToString(); //We will remove that I After we changed all our old materials with the new ones
            }
            // Feedback Over

            
            
        }
        
        
        
        if (GUILayout.Button("ReplaceMaterials"))
        {
            prefabsWithoutMaterials = Selection.gameObjects;
            foreach (GameObject prefab in prefabsWithoutMaterials)
            {

                
                Material[] matsArray = prefab.GetComponentInChildren<MeshRenderer>().sharedMaterials;
                int length = matsArray.Length;
                int childCount = prefab.transform.childCount;
                for (int i = 0; i < matsArray.Length; i++)
                {
                    for (int j = 0; j < targetMaterials.Length; j++)
                    {
                        if (targetMaterials[j].name.Insert(1, "I").ToString() == matsArray[i].name)
                        {
                            Debug.Log("Match Found");
                            //prefab.GetComponentInChildren<MeshRenderer>().sharedMaterials[i] = targetMaterials[j];
                            for (int c = 0; c < childCount; c++)
                            {
                                Debug.Log("Changed Child: " + c);
                                //if (i < prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials.Length) Added this thinking some lods have less materials but having an error message pop up is better than missing it and forgetting some without changing
                                //prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials[i] = targetMaterials[j];

                                //prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials[i] = targetMaterials[j];
                                 Material[] fakeArray = prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials;
                                 prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials.CopyTo(fakeArray, 0);
                                 fakeArray[i] = targetMaterials[j];
                                
                                prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials = fakeArray;





                                Debug.Log(prefab.transform.GetChild(c).GetComponent<MeshRenderer>().sharedMaterials[i].name);
                            }
                        }

                    }
                }

                Debug.Log("Materials of " + prefab.name + "have been changed.");
            }

          
        }

        GUILayout.Label("\n!! Important Note !!", EditorStyles.boldLabel);
        GUILayout.Label("\nIn order for the material replacer to work, all of your unreal materials must be named as:\nMI_MymateRial12 and all of your Unity materials \nmust be named as: M_MymateRial12 // Also the prefabs must be empty parents with children meshes inside", EditorStyles.wordWrappedLabel);

        GUILayout.Label("Note: This Tool is still experimental and it was made by Berkcan Akca for Leartes Studios, \nDo not use it outside of the office works and don't forget to delete it before submitting your work\n to the asset store.\n   <| . |>\n      __", EditorStyles.boldLabel);
    }
}
