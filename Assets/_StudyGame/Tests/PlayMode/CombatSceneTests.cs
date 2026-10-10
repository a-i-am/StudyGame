using System;
using System.Collections;
using System.Reflection;
using Cinemachine;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace StudyGame.Combat.Tests
{
    public class CombatSceneTests
    {
        const string ScenePath = "Assets/_StudyGame/Scenes/Adventure Field Prototype.unity";

        PlayerLocomotion _player;
        ActionCameraRig _rig;
        CinemachineBrain _brain;
        Gamepad _pad;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            Time.captureFramerate = 30;
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            yield return null;
            _player = UnityEngine.Object.FindFirstObjectByType<PlayerLocomotion>();
            _rig = UnityEngine.Object.FindFirstObjectByType<ActionCameraRig>();
            _brain = UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>();
            Assert.IsNotNull(_player);
            Assert.IsNotNull(_rig);
            Assert.IsNotNull(_brain);
        }

        [TearDown]
        public void Restore()
        {
            Time.captureFramerate = 0;
            if (_pad != null) InputSystem.RemoveDevice(_pad);
            _pad = null;
        }

        [UnityTest]
        public IEnumerator ExploreViewMatchesEasyStart()
        {
            Transform target = _player.transform.Find("CameraTarget");
            Camera output = _brain.OutputCamera;
            Assert.AreEqual(1.45f, target.localPosition.y, 1e-3f);
            Assert.AreEqual(30f, Mathf.DeltaAngle(0f, target.eulerAngles.x), 0.5f);
            Assert.AreEqual(FovMath.VerticalFovForAspect(60f, 16f / 9f, output.aspect), output.fieldOfView, 0.5f);

            Vector3 expected = target.position + Quaternion.Euler(0f, target.eulerAngles.y, 0f) * new Vector3(0f, 1.5f, -4f);
            Assert.Less(Vector3.Distance(expected, output.transform.position), 0.1f, "camera " + output.transform.position);
            yield break;
        }

        [UnityTest]
        public IEnumerator DashIsInvulnerableThenReturnsToExploreCamera()
        {
            Assert.AreEqual("CM_Explore", _brain.ActiveVirtualCamera.Name);
            Assert.IsFalse(_player.IsInvulnerable);

            Assert.IsTrue(_player.TryDash(_player.transform.forward));
            Assert.IsTrue(_player.IsInvulnerable);
            yield return null;
            yield return null;
            Assert.AreEqual("CM_Dash", _brain.ActiveVirtualCamera.Name);

            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(_player.IsDashing);
            Assert.IsFalse(_player.IsInvulnerable);
            Assert.AreEqual("CM_Explore", _brain.ActiveVirtualCamera.Name);
            Assert.IsFalse(_brain.IsBlending);
        }

        [UnityTest]
        public IEnumerator DashTravelsFixedDistance()
        {
            Vector3 start = _player.transform.position;
            Assert.IsTrue(_player.TryDash(_player.transform.forward));
            int guard = 0;
            while (_player.IsDashing && guard++ < 60) yield return null;

            Vector3 moved = _player.transform.position - start;
            moved.y = 0f;
            Assert.AreEqual(5f, moved.magnitude, 0.05f);
        }

        [UnityTest]
        public IEnumerator EasyStartRigIsDisabled()
        {
            Behaviour legacy = _player.GetComponent("ThirdPersonController") as Behaviour;
            Assert.IsNotNull(legacy);
            Assert.IsFalse(legacy.enabled);
            int found = 0;
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name != "CameraController" && root.name != "ThirdPersonController") continue;
                found++;
                Assert.IsFalse(root.activeSelf, root.name);
            }
            Assert.AreEqual(2, found);
            yield break;
        }

        [UnityTest]
        public IEnumerator CameraStaysInFrontOfWallBehindPlayer()
        {
            Transform target = _player.transform.Find("CameraTarget");
            Vector3 back = -target.forward;
            back.y = 0f;
            back.Normalize();

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = target.position + back * 1.5f;
            wall.transform.rotation = Quaternion.LookRotation(back);
            wall.transform.localScale = new Vector3(8f, 8f, 0.3f);
            Physics.SyncTransforms();
            Assert.IsTrue(Physics.Linecast(target.position, target.position + back * 4f, out RaycastHit pre)
                && pre.collider.gameObject == wall);

            yield return null;
            yield return null;
            yield return null;

            Vector3 cameraPosition = _brain.transform.position;
            bool blocked = Physics.Linecast(cameraPosition, target.position, out RaycastHit hit)
                && hit.collider.gameObject == wall;
            Assert.IsFalse(blocked, "camera " + cameraPosition);
            Assert.Less(Vector3.Dot(cameraPosition - target.position, back), 1.5f);
        }

        [UnityTest]
        public IEnumerator UpdateLoopsDoNotAllocate()
        {
            _pad = InputSystem.AddDevice<Gamepad>();
            GamepadState state = new GamepadState { leftStick = Vector2.up, rightStick = Vector2.right }
                .WithButton(GamepadButton.LeftStick);
            InputSystem.QueueStateEvent(_pad, state);
            InputSystem.Update();

            Action locomotion = Bind(_player, "Update");
            Action camera = Bind(_rig, "LateUpdate");
            FieldInfo lastAspect = typeof(ActionCameraRig).GetField("_lastAspect", BindingFlags.Instance | BindingFlags.NonPublic);
            locomotion();
            camera();

            lastAspect.SetValue(_rig, -1f);
            Assert.That(() => { locomotion(); camera(); }, Is.Not.AllocatingGCMemory());
            _player.TryDash(_player.transform.forward);
            Assert.That(() => { locomotion(); camera(); }, Is.Not.AllocatingGCMemory());
            yield break;
        }

        static Action Bind(MonoBehaviour target, string method)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            return (Action)Delegate.CreateDelegate(typeof(Action), target, info);
        }
    }
}
