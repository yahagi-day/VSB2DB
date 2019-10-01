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
        public GameObject Model = null;

        [MenuItem("VRM/VSB2DB")]

        private static void Create()
        {
            GetWindow<VSB2DBEditor>("VSB2DB");
        }
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Settings");
                Model = EditorGUILayout.ObjectField("Target Model", Model, typeof(GameObject), true) as GameObject;
                if (GUILayout.Button("Start VSB2DB"))
                {
                    DoVSB2DB(Model);
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
            dynamicbone.m_Gravity = springBone.m_gravityPower * springBone.m_gravityDir;
            dynamicbone.m_Elasticity = springBone.m_stiffnessForce * 0.05f;
            dynamicbone.m_Stiffness = springBone.m_stiffnessForce * 0.25f;
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