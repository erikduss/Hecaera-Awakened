using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(FPD_FoldFrameAttribute))]
    public class FPD_FoldFrame : PropertyDrawer
    {
        FPD_FoldFrameAttribute Attribute { get { return ((FPD_FoldFrameAttribute)base.attribute); } }
        SerializedProperty[] props = null;

        //private bool folded = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //EditorGUI.BeginProperty(position, label, property);

            if (Attribute == null) return;
            if (Attribute.VariablesToStore == null) return;

            if (props == null)
            {
                props = new SerializedProperty[Attribute.VariablesToStore.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    props[i] = property.serializedObject.FindProperty(Attribute.VariablesToStore[i]);
                }
            }

            GUILayout.Space(7);
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUI.indentLevel++;

            GUIStyle foldBold = EditorStyles.foldout;
            foldBold.fontStyle = FontStyle.Bold;
            Attribute.Folded = EditorGUILayout.Foldout(Attribute.Folded, " " + Attribute.FrameTitle, true, foldBold);

            if (Attribute.Folded)
            {
                GUILayout.Space(3);
                for (int i = 0; i < props.Length; i++)
                {
                    if (props[i] != null)
                        EditorGUILayout.PropertyField(props[i]);
                    else
                        EditorGUILayout.LabelField("Wrong property name?");
                }
            }

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            //EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float size = -EditorGUIUtility.singleLineHeight / 5f;
            return size;
        }
    }

}

