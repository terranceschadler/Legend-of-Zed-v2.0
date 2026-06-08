using LegendOfZed.Enemies;
using TopDownShooter;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Editor
{
    public static class ZedV11ZombieRagdollSetup
    {
        private const string ZombieRootName = "Zed_Prototype_Zombie_Enemy";

        [MenuItem("Legend of Zed/Setup/v1.1 Rebuild Tuned Zombie Ragdoll")]
        public static void RebuildTunedZombieRagdoll()
        {
            GameObject zombieRoot = GameObject.Find(ZombieRootName);
            if (zombieRoot == null)
            {
                Debug.LogWarning("Could not find " + ZombieRootName + " in the open scene.");
                return;
            }

            RemoveDemoHitPoints(zombieRoot);
            RemoveBadNavMeshAgentIfNeeded(zombieRoot);

            ZedPrototypeZombieEnemy brain = zombieRoot.GetComponent<ZedPrototypeZombieEnemy>();
            if (brain == null)
            {
                brain = zombieRoot.AddComponent<ZedPrototypeZombieEnemy>();
            }

            brain.RagdollOnDeath = true;
            brain.DestroyOnDeath = false;
            brain.DeathDisableDelay = 999f;

            Animator animator = zombieRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                Debug.LogWarning("Zombie has no Animator. Cannot build ragdoll.");
                return;
            }

            brain.Animator = animator;

            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            Transform upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);

            Transform leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            Transform rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

            Transform leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            Transform rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Transform rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            Transform chestTarget = upperChest != null ? upperChest : head;
            Transform spineTarget = chest != null ? chest : chestTarget;

            SetupCapsuleBone(hips, spine != null ? spine : chestTarget, null, 0.16f, 5.0f);
            SetupCapsuleBone(spine, spineTarget, hips, 0.14f, 3.0f);
            SetupCapsuleBone(chest, chestTarget, spine != null ? spine : hips, 0.15f, 3.0f);
            SetupCapsuleBone(upperChest, head, chest != null ? chest : spine, 0.14f, 2.5f);

            SetupSphereBone(head, chestTarget != head ? chestTarget : (upperChest != null ? upperChest : chest), 0.13f, 1.0f);

            SetupCapsuleBone(leftUpperArm, leftLowerArm, chest != null ? chest : upperChest, 0.055f, 0.9f);
            SetupCapsuleBone(leftLowerArm, leftHand, leftUpperArm, 0.045f, 0.7f);
            SetupSphereBone(leftHand, leftLowerArm, 0.045f, 0.35f);

            SetupCapsuleBone(rightUpperArm, rightLowerArm, chest != null ? chest : upperChest, 0.055f, 0.9f);
            SetupCapsuleBone(rightLowerArm, rightHand, rightUpperArm, 0.045f, 0.7f);
            SetupSphereBone(rightHand, rightLowerArm, 0.045f, 0.35f);

            SetupCapsuleBone(leftUpperLeg, leftLowerLeg, hips, 0.075f, 1.8f);
            SetupCapsuleBone(leftLowerLeg, leftFoot, leftUpperLeg, 0.06f, 1.2f);
            SetupSphereBone(leftFoot, leftLowerLeg, 0.06f, 0.45f);

            SetupCapsuleBone(rightUpperLeg, rightLowerLeg, hips, 0.075f, 1.8f);
            SetupCapsuleBone(rightLowerLeg, rightFoot, rightUpperLeg, 0.06f, 1.2f);
            SetupSphereBone(rightFoot, rightLowerLeg, 0.06f, 0.45f);

            DisableRagdollNow(zombieRoot);

            EditorUtility.SetDirty(zombieRoot);
            EditorUtility.SetDirty(brain);
            Debug.Log("Rebuilt tuned zombie ragdoll. Colliders are aligned bone-to-child. DeathDisableDelay remains 999 for testing.");
        }

        private static void RemoveDemoHitPoints(GameObject root)
        {
            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                Object.DestroyImmediate(hitPoints[i], true);
            }
        }

        private static void RemoveBadNavMeshAgentIfNeeded(GameObject root)
        {
            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                return;
            }

            NavMeshHit hit;
            bool hasNavMesh = NavMesh.SamplePosition(root.transform.position, out hit, 25f, NavMesh.AllAreas);
            if (!hasNavMesh || !agent.isOnNavMesh)
            {
                Object.DestroyImmediate(agent, true);
            }
        }

        private static void SetupCapsuleBone(Transform bone, Transform childBone, Transform connectedBone, float radius, float mass)
        {
            if (bone == null)
            {
                return;
            }

            CleanupRagdollParts(bone);

            Rigidbody rb = EnsureBody(bone, mass);

            CapsuleCollider capsule = bone.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = radius;
            capsule.enabled = false;

            Vector3 localEnd = childBone != null ? bone.InverseTransformPoint(childBone.position) : Vector3.up * (radius * 4f);
            if (localEnd.sqrMagnitude < 0.0001f)
            {
                localEnd = Vector3.up * (radius * 4f);
            }

            int axis = LargestAbsAxis(localEnd);
            float length = Mathf.Abs(GetAxisValue(localEnd, axis));
            capsule.direction = axis;
            capsule.height = Mathf.Max(length, radius * 2.25f);
            capsule.center = localEnd * 0.5f;

            SetupJoint(bone, connectedBone);
        }

        private static void SetupSphereBone(Transform bone, Transform connectedBone, float radius, float mass)
        {
            if (bone == null)
            {
                return;
            }

            CleanupRagdollParts(bone);

            EnsureBody(bone, mass);

            SphereCollider sphere = bone.gameObject.AddComponent<SphereCollider>();
            sphere.radius = radius;
            sphere.center = Vector3.zero;
            sphere.enabled = false;

            SetupJoint(bone, connectedBone);
        }

        private static Rigidbody EnsureBody(Transform bone, float mass)
        {
            Rigidbody rb = bone.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bone.gameObject.AddComponent<Rigidbody>();
            }

            rb.mass = mass;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.2f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return rb;
        }

        private static void SetupJoint(Transform bone, Transform connectedBone)
        {
            if (bone == null || connectedBone == null)
            {
                return;
            }

            Rigidbody connectedBody = connectedBone.GetComponent<Rigidbody>();
            if (connectedBody == null)
            {
                connectedBody = EnsureBody(connectedBone, 2f);
            }

            CharacterJoint joint = bone.gameObject.AddComponent<CharacterJoint>();
            joint.connectedBody = connectedBody;
            joint.enableProjection = true;
            joint.projectionDistance = 0.12f;
            joint.projectionAngle = 15f;
            joint.enablePreprocessing = false;

            SoftJointLimit lowTwist = joint.lowTwistLimit;
            lowTwist.limit = -35f;
            joint.lowTwistLimit = lowTwist;

            SoftJointLimit highTwist = joint.highTwistLimit;
            highTwist.limit = 35f;
            joint.highTwistLimit = highTwist;

            SoftJointLimit swing1 = joint.swing1Limit;
            swing1.limit = 45f;
            joint.swing1Limit = swing1;

            SoftJointLimit swing2 = joint.swing2Limit;
            swing2.limit = 35f;
            joint.swing2Limit = swing2;
        }

        private static void CleanupRagdollParts(Transform bone)
        {
            CharacterJoint[] joints = bone.GetComponents<CharacterJoint>();
            for (int i = 0; i < joints.Length; i++)
            {
                Object.DestroyImmediate(joints[i], true);
            }

            CapsuleCollider[] capsules = bone.GetComponents<CapsuleCollider>();
            for (int i = 0; i < capsules.Length; i++)
            {
                Object.DestroyImmediate(capsules[i], true);
            }

            SphereCollider[] spheres = bone.GetComponents<SphereCollider>();
            for (int i = 0; i < spheres.Length; i++)
            {
                Object.DestroyImmediate(spheres[i], true);
            }
        }

        private static int LargestAbsAxis(Vector3 value)
        {
            float x = Mathf.Abs(value.x);
            float y = Mathf.Abs(value.y);
            float z = Mathf.Abs(value.z);

            if (x >= y && x >= z)
            {
                return 0;
            }

            if (y >= x && y >= z)
            {
                return 1;
            }

            return 2;
        }

        private static float GetAxisValue(Vector3 value, int axis)
        {
            switch (axis)
            {
                case 0:
                    return value.x;
                case 1:
                    return value.y;
                default:
                    return value.z;
            }
        }

        private static void DisableRagdollNow(GameObject root)
        {
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (body.transform == root.transform)
                {
                    body.isKinematic = true;
                    body.useGravity = false;
                    continue;
                }

                body.isKinematic = true;
                body.useGravity = false;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider.transform == root.transform)
                {
                    continue;
                }

                collider.enabled = false;
            }
        }
    }
}
