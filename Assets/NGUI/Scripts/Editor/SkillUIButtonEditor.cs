using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(SkillUIButton))]
#else
[CustomEditor(typeof(SkillUIButton), true)]
#endif
public class SkillUIButtonEditor : UIButtonEditor
{
    protected override void DrawProperties()
    {
        base.DrawProperties();
        NGUIEditorTools.DrawProperty("ChangeTarget", serializedObject, "UseSwipeChangeTarget");
        NGUIEditorTools.DrawProperty("ChangeSkill", serializedObject, "UseSwipChangeSkill");
    }
}
