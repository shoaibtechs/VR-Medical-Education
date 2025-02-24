namespace MAGES.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Checks and applies Physics Settings.
    /// </summary>
    internal class PhysicsSettingsConfigurationTask : BaseConfigurationTask
    {
        /// <inheritdoc/>
        public override string Title => "Physics Settings";

        /// <inheritdoc/>
        public override string Message => "Physics settings are not optimized.";

        /// <inheritdoc/>
        public override string FixingMessage => "Updating Settings...";

        /// <inheritdoc/>
        public override string FixedMessage => "Physics settings were updated successfully.";

        /// <inheritdoc/>
        public override int Priority => 100;

        /// <inheritdoc/>
        public override void Load()
        {
            UpdateType(ConfigurationTaskType.Warning);
            UpdateState(ConfigurationTaskState.Loading);

            if (CheckConfiguration())
            {
                UpdateState(ConfigurationTaskState.Completed);
            }
            else
            {
                UpdateState(ConfigurationTaskState.Available);
            }
        }

        /// <inheritdoc/>
        public override void Fix()
        {
            if (State == ConfigurationTaskState.Completed || State == ConfigurationTaskState.InProgress)
            {
                return;
            }

            UpdateState(ConfigurationTaskState.InProgress);

            if (UpdateSettings())
            {
                UpdateState(ConfigurationTaskState.Completed);
            }
            else
            {
                UpdateState(ConfigurationTaskState.Available);
            }
        }

        private bool CheckConfiguration()
        {
            SerializedObject dynamicsManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/DynamicsManager.asset"));
            SerializedObject timeManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TimeManager.asset"));

            if (dynamicsManager == null || timeManager == null)
            {
                return false;
            }

            // All the serialized properties we want to access
            SerializedProperty fixedDeltaTime = timeManager.FindProperty("Fixed Timestep");
            SerializedProperty defaultSolverIterations = dynamicsManager.FindProperty("m_DefaultSolverIterations");
            SerializedProperty defaultSolverVelocityIterations = dynamicsManager.FindProperty("m_DefaultSolverVelocityIterations");
            SerializedProperty adaptiveForce = dynamicsManager.FindProperty("m_EnableAdaptiveForce");
            SerializedProperty frictionType = dynamicsManager.FindProperty("m_FrictionType");
            SerializedProperty solverType = dynamicsManager.FindProperty("m_SolverType");
            SerializedProperty defaultMaxAngularSpeed = dynamicsManager.FindProperty("m_DefaultMaxAngularSpeed");

            return
                fixedDeltaTime != null && fixedDeltaTime.floatValue <= 0.01f &&
                defaultSolverIterations != null && defaultSolverIterations.intValue == 25 &&
                defaultSolverVelocityIterations != null && defaultSolverVelocityIterations.intValue == 15 &&
                adaptiveForce != null && adaptiveForce.boolValue == true &&
                frictionType != null && frictionType.intValue == 1 &&
                solverType != null && solverType.intValue == 1 &&
                defaultMaxAngularSpeed != null && defaultMaxAngularSpeed.floatValue >= 25 &&
                LayerMask.NameToLayer("Hands") == 10;
        }

        private bool UpdateSettings()
        {
            if (EditorUtility.DisplayDialog("Warning", "The following settings will be applied:\nFixed Timestep -> 0.01\nDefault Solver Iterations -> 25\nDefault Solver Velocity Iterations -> 15\nEnable Adaptive Force -> true\nFriction Type -> One Directional Friction Type\nSolver Type -> Teporal Gauss Seidel\nDefault Max Angular Speed -> >=25\nCreate layer \"Hands\" in User Layer 10.", "Continue", "Cancel"))
            {
                SerializedObject dynamicsManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/DynamicsManager.asset"));
                SerializedObject timeManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TimeManager.asset"));

                if (dynamicsManager == null || timeManager == null)
                {
                    return false;
                }

                // All the serialized properties we want to access
                SerializedProperty fixedDeltaTime = timeManager.FindProperty("Fixed Timestep");
                SerializedProperty defaultSolverIterations = dynamicsManager.FindProperty("m_DefaultSolverIterations");
                SerializedProperty defaultSolverVelocityIterations = dynamicsManager.FindProperty("m_DefaultSolverVelocityIterations");
                SerializedProperty adaptiveForce = dynamicsManager.FindProperty("m_EnableAdaptiveForce");
                SerializedProperty frictionType = dynamicsManager.FindProperty("m_FrictionType");
                SerializedProperty solverType = dynamicsManager.FindProperty("m_SolverType");
                SerializedProperty defaultMaxAngularSpeed = dynamicsManager.FindProperty("m_DefaultMaxAngularSpeed");

                bool success = true;

                if (fixedDeltaTime != null)
                {
                    fixedDeltaTime.floatValue = Mathf.Min(fixedDeltaTime.floatValue, 0.01f);
                }
                else
                {
                    success = false;
                }

                if (defaultSolverIterations != null)
                {
                    defaultSolverIterations.intValue = 25;
                }
                else
                {
                    success = false;
                }

                if (defaultSolverVelocityIterations != null)
                {
                    defaultSolverVelocityIterations.intValue = 15;
                }
                else
                {
                    success = false;
                }

                if (adaptiveForce != null)
                {
                    adaptiveForce.boolValue = true;
                }
                else
                {
                    success = false;
                }

                if (frictionType != null)
                {
                    frictionType.boolValue = true;
                }
                else
                {
                    success = false;
                }

                if (solverType != null)
                {
                    solverType.intValue = 1;
                }
                else
                {
                    success = false;
                }

                if (defaultMaxAngularSpeed != null)
                {
                    defaultMaxAngularSpeed.floatValue = Mathf.Max(defaultMaxAngularSpeed.floatValue, 25f);
                }
                else
                {
                    success = false;
                }

                timeManager.ApplyModifiedProperties();
                dynamicsManager.ApplyModifiedProperties();

                if (!CreateLayer(10, "Hands"))
                {
                    if (!(EditorUtility.DisplayDialog("Warning", $"The User Layer 10 is already taken by layer {LayerMask.LayerToName(10)}. Do you want to overwrite it?.", "No, I will create the layer manually later", "Yes") && CreateLayer(10, "Hands", true)))
                    {
                        success = false;
                    }
                }

                if (!success)
                {
                    EditorUtility.DisplayDialog("Error", "Some settings could not be applied. Please apply them manually.", "Ok");
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a layer in the first available spot.
        /// </summary>
        /// <param name="name">The name of the layer.</param>
        /// <returns>The layer index or -1 if no available spots found.</returns>
        private int CreateLayer(string name)
        {
            int foundLayerIndex = LayerMask.NameToLayer(name);
            if (foundLayerIndex != -1)
            {
                return foundLayerIndex;
            }

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));

            SerializedProperty layersArray = tagManager.FindProperty("layers");

            for (int i = 0; i < layersArray.arraySize; i++)
            {
                if (string.IsNullOrEmpty(layersArray.GetArrayElementAtIndex(i).stringValue))
                {
                    layersArray.GetArrayElementAtIndex(i).stringValue = name;
                    tagManager.ApplyModifiedProperties();
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates a layer in the given index.
        /// </summary>
        /// <param name="index">The layer index.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="force">If true, the layer will be created even if another layer exists in the given <paramref name="index"/>.</param>
        /// <returns>True on success.</returns>
        private bool CreateLayer(int index, string name, bool force = false)
        {
            string prevVal = LayerMask.LayerToName(index);
            if (prevVal != string.Empty && prevVal != name && !force)
            {
                return false;
            }

            if (prevVal == name)
            {
                return true;
            }

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));

            SerializedProperty layersArray = tagManager.FindProperty("layers");
            if (index >= layersArray.arraySize)
            {
                return false;
            }

            SerializedProperty p = layersArray.GetArrayElementAtIndex(index);
            p.stringValue = name;
            tagManager.ApplyModifiedProperties();
            return true;
        }
    }
}
