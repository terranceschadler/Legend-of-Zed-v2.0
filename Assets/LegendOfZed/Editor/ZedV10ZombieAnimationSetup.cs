using System.Collections.Generic;
using System.IO;
using LegendOfZed.Enemies;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedV10ZombieAnimationSetup
    {
        private const string AnimationRoot = "Assets/Land Of The Dead - Road to Fiddler's Green";
        private const string GeneratedFolder = "Assets/LegendOfZed/Generated/Animators";
        private const string ControllerPath = GeneratedFolder + "/Zed_Prototype_Zombie_LandOfTheDead.controller";
        private const string ZombieRootName = "Zed_Prototype_Zombie_Enemy";

        [MenuItem("Legend of Zed/Setup/v1.0 Build Zombie Animator From Land Of The Dead Clips")]
        public static void BuildZombieAnimatorFromLandOfTheDeadClips()
        {
            EnsureGeneratedFolder();

            AnimationClip idle = FindClipByExactFbxName("bored");
            AnimationClip walk = FindClipByExactFbxName("walk");
            AnimationClip run = FindClipByExactFbxName("run");
            AnimationClip attackBite = FindClipByExactFbxName("atk bite");
            AnimationClip attackLeft = FindClipByExactFbxName("atk left");
            AnimationClip attackRight = FindClipByExactFbxName("atk right");
            AnimationClip attackRight2 = FindClipByExactFbxName("atk right2");
            AnimationClip attackTwoHand = FindClipByExactFbxName("atk two hand");

            if (idle == null || walk == null || run == null)
            {
                Debug.LogWarning("v1.0 zombie animation setup needs exact FBX filenames bored, walk, and run under " + AnimationRoot + ". Missing:" +
                                 MissingLabel("bored", idle) + MissingLabel("walk", walk) + MissingLabel("run", run));
                return;
            }

            SetClipLooping("bored", true);
            SetClipLooping("walk", true);
            SetClipLooping("run", true);
            SetClipLooping("atk bite", false);
            SetClipLooping("atk left", false);
            SetClipLooping("atk right", false);
            SetClipLooping("atk right2", false);
            SetClipLooping("atk two hand", false);

            idle = FindClipByExactFbxName("bored");
            walk = FindClipByExactFbxName("walk");
            run = FindClipByExactFbxName("run");
            attackBite = FindClipByExactFbxName("atk bite");
            attackLeft = FindClipByExactFbxName("atk left");
            attackRight = FindClipByExactFbxName("atk right");
            attackRight2 = FindClipByExactFbxName("atk right2");
            attackTwoHand = FindClipByExactFbxName("atk two hand");

            AnimatorController controller = CreateOrReplaceController();
            ConfigureController(controller, idle, walk, run, attackBite, attackLeft, attackRight, attackRight2, attackTwoHand);
            AssignControllerToPrototypeZombie(controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.0 zombie animator built from exact FBX filenames in " + AnimationRoot + ". Idle=bored looped, Walk=walk looped, Run=run looped, attacks are one-shot. It will not use 'run on four' for Run. Player, weapons, bullets, WeaponData, ammo, projectile IDs, and v0.9 feedback were not changed.");
        }

        private static string MissingLabel(string label, Object asset)
        {
            return asset == null ? " " + label : string.Empty;
        }

        private static void EnsureGeneratedFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/LegendOfZed"))
            {
                AssetDatabase.CreateFolder("Assets", "LegendOfZed");
            }

            if (!AssetDatabase.IsValidFolder("Assets/LegendOfZed/Generated"))
            {
                AssetDatabase.CreateFolder("Assets/LegendOfZed", "Generated");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            {
                AssetDatabase.CreateFolder("Assets/LegendOfZed/Generated", "Animators");
            }
        }

        private static AnimationClip FindClipByExactFbxName(string wantedName)
        {
            string path = FindExactModelPath(wantedName);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("v1.0 zombie animation setup could not find exact FBX/model filename: " + wantedName);
                return null;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                AnimationClip clip = asset as AnimationClip;
                if (clip == null || clip.name.StartsWith("__preview__"))
                {
                    continue;
                }

                return clip;
            }

            Debug.LogWarning("v1.0 zombie animation setup found " + wantedName + " but no usable AnimationClip inside it.");
            return null;
        }

        private static string FindExactModelPath(string wantedName)
        {
            if (!AssetDatabase.IsValidFolder(AnimationRoot))
            {
                Debug.LogWarning("v1.0 zombie animation setup could not find animation folder: " + AnimationRoot);
                return null;
            }

            string wanted = Normalize(wantedName);
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { AnimationRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Normalize(Path.GetFileNameWithoutExtension(path));
                if (fileName == wanted)
                {
                    return path;
                }
            }

            return null;
        }

        private static void SetClipLooping(string fbxName, bool loop)
        {
            string path = FindExactModelPath(fbxName);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }

            bool changed = false;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].loopTime != loop)
                {
                    clips[i].loopTime = loop;
                    changed = true;
                }

                if (clips[i].loopPose != loop)
                {
                    clips[i].loopPose = loop;
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            importer.clipAnimations = clips;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static string Normalize(string value)
        {
            return value.ToLowerInvariant().Replace("_", " ").Replace("-", " ").Trim();
        }

        private static AnimatorController CreateOrReplaceController()
        {
            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            return AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        private static void ConfigureController(
            AnimatorController controller,
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip attackBite,
            AnimationClip attackLeft,
            AnimationClip attackRight,
            AnimationClip attackRight2,
            AnimationClip attackTwoHand)
        {
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("AttackIndex", AnimatorControllerParameterType.Int);
            controller.AddParameter("AttackBite", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackLeft", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackRight", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackRight2", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackTwoHand", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            stateMachine.states = new ChildAnimatorState[0];
            stateMachine.anyStateTransitions = new AnimatorStateTransition[0];

            AnimatorState idleState = stateMachine.AddState("Idle_Bored");
            idleState.motion = idle;
            stateMachine.defaultState = idleState;

            AnimatorState walkState = stateMachine.AddState("Walk");
            walkState.motion = walk;

            AnimatorState runState = stateMachine.AddState("Run_Chase");
            runState.motion = run;

            AddSpeedTransition(idleState, walkState, 0.05f, true);
            AddSpeedTransition(walkState, idleState, 0.05f, false);
            AddSpeedTransition(walkState, runState, 1.7f, true);
            AddSpeedTransition(runState, walkState, 1.7f, false);
            AddSpeedTransition(idleState, runState, 1.7f, true);
            AddSpeedTransition(runState, idleState, 0.05f, false);

            Dictionary<string, AnimationClip> attacks = new Dictionary<string, AnimationClip>
            {
                { "Attack_Bite", attackBite },
                { "Attack_Left", attackLeft },
                { "Attack_Right", attackRight },
                { "Attack_Right2", attackRight2 },
                { "Attack_TwoHand", attackTwoHand }
            };

            foreach (KeyValuePair<string, AnimationClip> pair in attacks)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                AnimatorState attackState = stateMachine.AddState(pair.Key);
                attackState.motion = pair.Value;
                attackState.speed = 1f;

                string triggerName = TriggerNameForState(pair.Key);
                AnimatorStateTransition attackTransition = stateMachine.AddAnyStateTransition(attackState);
                attackTransition.hasExitTime = false;
                attackTransition.duration = 0.05f;
                attackTransition.canTransitionToSelf = false;
                attackTransition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);

                AnimatorStateTransition returnTransition = attackState.AddTransition(idleState);
                returnTransition.hasExitTime = true;
                returnTransition.exitTime = 0.9f;
                returnTransition.duration = 0.12f;
            }

            EditorUtility.SetDirty(controller);
        }

        private static string TriggerNameForState(string stateName)
        {
            switch (stateName)
            {
                case "Attack_Bite": return "AttackBite";
                case "Attack_Left": return "AttackLeft";
                case "Attack_Right": return "AttackRight";
                case "Attack_Right2": return "AttackRight2";
                default: return "AttackTwoHand";
            }
        }

        private static void AddSpeedTransition(AnimatorState from, AnimatorState to, float threshold, bool greater)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.12f;
            transition.AddCondition(greater ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, "Speed");
        }

        private static void AssignControllerToPrototypeZombie(AnimatorController controller)
        {
            GameObject zombie = GameObject.Find(ZombieRootName);
            if (zombie == null)
            {
                Debug.LogWarning("v1.0 zombie animation setup built the controller but could not find " + ZombieRootName + " in the open scene. Run the zombie enemy setup first, then run this again.");
                return;
            }

            ZedPrototypeZombieEnemy brain = zombie.GetComponent<ZedPrototypeZombieEnemy>();
            Animator animator = brain != null && brain.Animator != null ? brain.Animator : zombie.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("v1.0 zombie animation setup could not find an Animator under " + ZombieRootName + ".");
                return;
            }

            animator.runtimeAnimatorController = controller;
            if (brain != null)
            {
                brain.Animator = animator;
                EditorUtility.SetDirty(brain);
            }

            EditorUtility.SetDirty(animator);
        }
    }
}