using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using VRM;

/* -------------------------------------------------
 * VRMSpringBone to DynamicBone Converter. ver0.1.0 
 * Created by yahagi_day
 * 
 * Address
 *  twitter:@yahagi_day (Quick reaction)
 *  mail:yahagi.day@tensi.org
 --------------------------------------------------*/
 namespace VSB2DB
{
    public class VSB2DBEditor : EditorWindow
    {
        public class ConvertParams
        {
            public GameObject Model { get; set; } = null;
            public bool Gravity_Option { get; set; } = false;
            public float Inert { get; set; } = 0.0f;
            public float UpdateRate { get; set; } = 60.0f;
            public Vector3 Force { get; set; } = Vector3.zero;
            public float Stiffness { get; set; } = 0.2f;
            public DynamicBone.UpdateMode UpdateMode { get; set; } = DynamicBone.UpdateMode.Normal;
        }
        [MenuItem("VRM/VSB2DB")]

        private static void Create()
        {
            GetWindow<VSB2DBEditor>("VSB2DB");
        }

        private ConvertParams Params = new ConvertParams();

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Settings");
                EditorGUI.indentLevel++;
                {
                    Params.Model = EditorGUILayout.ObjectField("Target Model", Params.Model, typeof(GameObject), true) as GameObject;
                    Params.UpdateRate = EditorGUILayout.FloatField("UpdateRate", Params.UpdateRate);
                    Params.UpdateMode = (DynamicBone.UpdateMode)EditorGUILayout.EnumPopup("Update Mode", Params.UpdateMode);
                    Params.Gravity_Option = EditorGUILayout.Toggle("Gravity conversion", Params.Gravity_Option);
                    Params.Stiffness = EditorGUILayout.Slider("Stiffness Params", Params.Stiffness, 0.0f, 1.0f);
                    Params.Inert = EditorGUILayout.Slider("Inert Param", Params.Inert, 0.0f, 1.0f);
                    Params.Force = EditorGUILayout.Vector3Field("Force Params", Params.Force);
                }
                if (GUILayout.Button("Convert"))
                {
                    DoVSB2DB(Params.Model);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private Component[] VRMSpringBones;
        private Component[] VRMSpringBoneColliderGrope;
        private void DoVSB2DB(GameObject Model)
        {
            VRMSpringBoneColliderGrope = Model.GetComponentsInChildren<VRMSpringBoneColliderGroup>();
            foreach (VRMSpringBoneColliderGroup colliderGroup in VRMSpringBoneColliderGrope)
            {
                var obj = colliderGroup.gameObject;
                VRMSpringBoneColliderGroup.SphereCollider[] colliders = colliderGroup.Colliders;
                
                foreach (var item in colliders)
                {
                    AddDynamicBonecollider(obj, item);
                }
            }

            Debug.Log("AddDynamicBonecollider");

            VRMSpringBones = Model.GetComponentsInChildren<VRMSpringBone>();
            foreach (VRMSpringBone springBone in VRMSpringBones)
            {
                foreach (var item in springBone.RootBones)
                {
                    AddDynamicBone(springBone, item);
                }

                DestroyImmediate(springBone);
            }
            Debug.Log("Converted VRMSpringBone to DynamicBone");

            foreach (VRMSpringBoneColliderGroup colliderGroup in VRMSpringBoneColliderGrope)
                DestroyImmediate(colliderGroup);
            Debug.Log("Delete VRMSpringBoneColider");

            Debug.Log("End Task");
        }

        private void AddDynamicBonecollider(GameObject obj, VRMSpringBoneColliderGroup.SphereCollider collider)
        {
            var dynamicbonecollider = obj.AddComponent<DynamicBoneCollider>();
            dynamicbonecollider.m_Radius = collider.Radius;
            dynamicbonecollider.m_Center = collider.Offset;
        }

        private void AddDynamicBone(VRMSpringBone springBone, Transform Root)
        {
            var dynamicbone = springBone.gameObject.AddComponent<DynamicBone>();
            dynamicbone.m_Root = Root;
            dynamicbone.m_UpdateRate = Params.UpdateRate;
            dynamicbone.m_UpdateMode = Params.UpdateMode;
            if (Params.Gravity_Option)
            {
                dynamicbone.m_Gravity = springBone.m_gravityPower * springBone.m_gravityDir;
            }
            else
            {
                dynamicbone.m_Gravity = Vector3.zero;
            }
            dynamicbone.m_Elasticity = springBone.m_stiffnessForce * 0.25f;
            dynamicbone.m_Stiffness = Params.Stiffness;
            dynamicbone.m_Force = Params.Force;
            dynamicbone.m_Damping = springBone.m_dragForce * 0.6f;
            dynamicbone.m_Radius = springBone.m_hitRadius;
            dynamicbone.m_Colliders = new List<DynamicBoneColliderBase>();

            foreach (var colliderGroup in springBone.ColliderGroups)
            {

                DynamicBoneCollider[] colliders = colliderGroup.gameObject.GetComponents<DynamicBoneCollider>();

                if (colliders != null)
                {
                    dynamicbone.m_Colliders.AddRange(colliders);
                }
            }
        }
    }
}