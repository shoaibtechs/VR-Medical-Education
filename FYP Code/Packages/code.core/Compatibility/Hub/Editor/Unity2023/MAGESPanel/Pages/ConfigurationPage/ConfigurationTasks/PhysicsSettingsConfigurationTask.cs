namespace MAGES.Editor
{
    using System;
    using Unity.IntegerTime;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

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

            if (dynamicsManager == null)
            {
                return false;
            }

            // All the serialized properties we want to access
            SerializedProperty defaultSolverIterations = dynamicsManager.FindProperty("m_DefaultSolverIterations");
            SerializedProperty defaultSolverVelocityIterations = dynamicsManager.FindProperty("m_DefaultSolverVelocityIterations");
            SerializedProperty adaptiveForce = dynamicsManager.FindProperty("m_EnableAdaptiveForce");
            SerializedProperty frictionType = dynamicsManager.FindProperty("m_FrictionType");
            SerializedProperty solverType = dynamicsManager.FindProperty("m_SolverType");
            SerializedProperty defaultMaxAngularSpeed = dynamicsManager.FindProperty("m_DefaultMaxAngularSpeed");

            return
                GetFixedTimeAsFloat() <= 0.01f &&
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

                if (dynamicsManager == null)
                {
                    return false;
                }

                // All the serialized properties we want to access
                SerializedProperty defaultSolverIterations = dynamicsManager.FindProperty("m_DefaultSolverIterations");
                SerializedProperty defaultSolverVelocityIterations = dynamicsManager.FindProperty("m_DefaultSolverVelocityIterations");
                SerializedProperty adaptiveForce = dynamicsManager.FindProperty("m_EnableAdaptiveForce");
                SerializedProperty frictionType = dynamicsManager.FindProperty("m_FrictionType");
                SerializedProperty solverType = dynamicsManager.FindProperty("m_SolverType");
                SerializedProperty defaultMaxAngularSpeed = dynamicsManager.FindProperty("m_DefaultMaxAngularSpeed");

                bool success = true;

                SetFixedTimeAsFloat(0.01f);

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

        private void SetFixedTimeAsFloat(float value)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/TimeManager.asset"));

            SerializedProperty m_FixedTimestepCountProperty;
            const float MinFixedTimeStep = 0.0001f;

            var fixedTimestepProperty = serializedObject.FindProperty("Fixed Timestep");
            m_FixedTimestepCountProperty = fixedTimestepProperty.FindPropertyRelative("m_Count");

            var numerator = fixedTimestepProperty.FindPropertyRelative("m_Rate.m_Numerator");
            var denominator = fixedTimestepProperty.FindPropertyRelative("m_Rate.m_Denominator");
            RationalTime.TicksPerSecond m_FixedTimeTicksPerSecond =
                new RationalTime.TicksPerSecond(numerator.uintValue, denominator.uintValue);

            var fixedTime = (float)new RationalTime(m_FixedTimestepCountProperty.longValue, m_FixedTimeTicksPerSecond).ToDouble(); // Convert a tick count to a float
            fixedTime = Mathf.Min(value, fixedTime);
            fixedTime = MathF.Max(fixedTime, MinFixedTimeStep);
            var newCount = RationalTime
                .FromDouble(fixedTime, m_FixedTimeTicksPerSecond)
                .Count; // convert it back to a count to store in the property
            m_FixedTimestepCountProperty.longValue = newCount;

            serializedObject.ApplyModifiedProperties();
        }

        private float GetFixedTimeAsFloat()
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/TimeManager.asset"));

            SerializedProperty m_FixedTimestepCountProperty;

            var fixedTimestepProperty = serializedObject.FindProperty("Fixed Timestep");
            m_FixedTimestepCountProperty = fixedTimestepProperty.FindPropertyRelative("m_Count");

            var numerator = fixedTimestepProperty.FindPropertyRelative("m_Rate.m_Numerator");
            var denominator = fixedTimestepProperty.FindPropertyRelative("m_Rate.m_Denominator");
            RationalTime.TicksPerSecond m_FixedTimeTicksPerSecond =
                new RationalTime.TicksPerSecond(numerator.uintValue, denominator.uintValue);

            var fixedTime = (float)new RationalTime(m_FixedTimestepCountProperty.longValue, m_FixedTimeTicksPerSecond).ToDouble(); // Convert a tick count to a float
            return fixedTime;
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
