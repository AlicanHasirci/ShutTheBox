namespace Player.Jokers.Editor
{
    using System;
    using Jokers;
    using Network;
    using UnityEditor;
    using Enum = System.Enum;

    [CustomEditor(typeof(JokerDatabase))]
    public class JokerDatabaseEditor : Editor
    {
        public void OnEnable()
        {
            bool changed = false;
            SerializedProperty property = serializedObject.FindProperty("_jokers");
            Array values = Enum.GetValues(typeof(Joker));
            if (property.arraySize != values.Length)
            {
                property.arraySize = values.Length;
                changed = true;
            }

            int index = 0;
            foreach (object value in values)
            {
                Joker joker = (Joker)value;
                SerializedProperty elementAtIndex = property.GetArrayElementAtIndex(index);
                JokerModel model = (JokerModel)elementAtIndex.boxedValue;
                if (model == null || model.Type != joker)
                {
                    elementAtIndex.boxedValue = new JokerModel()
                    {
                        Type = joker,
                        Icon = null,
                        Name = string.Empty,
                        Description = string.Empty,
                    };
                    changed = true;
                }
                index++;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
