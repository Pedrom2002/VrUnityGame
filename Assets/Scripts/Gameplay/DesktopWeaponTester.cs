using UnityEngine;
using UnityEngine.InputSystem;

namespace VRAimLab.Gameplay
{
    /// Desktop mode weapon tester - allows testing VR weapons without VR headset
    /// Attach to the Main Camera
    /// Controls: WASD to move, Mouse to look, Left Click to shoot, E to pickup weapon
    public class DesktopWeaponTester : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float cameraHeight = 1.6f; // Eye height in meters
        [SerializeField] private bool cursorLocked = true;

        [Header("Weapon Settings")]
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private Vector3 weaponHolderPosition = new Vector3(0.5f, -0.15f, 0.4f);
        [SerializeField] private Vector3 weaponHolderRotation = Vector3.zero;
        [SerializeField] private Vector3 weaponLocalPosition = Vector3.zero; // Weapon offset from holder
        [SerializeField] private Vector3 weaponLocalRotation = Vector3.zero; // Weapon rotation offset
        [SerializeField] private float weaponScale = 1f; // Weapon scale

        [Header("Pickup Settings")]
        [SerializeField] private float pickupRange = 5f; // Distance to pickup weapon
        [SerializeField] private bool showPickupRange = true; // Show pickup range gizmo
        [SerializeField] private LayerMask weaponLayer;

        private VRWeaponController currentWeapon;
        private float rotationX = 0f;
        private CharacterController characterController;

        private void Start()
        {
            // Set camera height (eye level)
            Vector3 currentPos = transform.position;
            currentPos.y = cameraHeight;
            transform.position = currentPos;

            Debug.Log($"[DesktopTester] Camera positioned at height: {cameraHeight}m (eye level)");

            // Create weapon holder if not assigned
            if (weaponHolder == null)
            {
                GameObject holder = new GameObject("WeaponHolder");
                holder.transform.SetParent(transform);
                // Use configurable position and rotation from Inspector
                holder.transform.localPosition = weaponHolderPosition;
                holder.transform.localRotation = Quaternion.Euler(weaponHolderRotation);
                weaponHolder = holder.transform;
            }
            else
            {
                // Update position if weaponHolder was assigned manually
                weaponHolder.localPosition = weaponHolderPosition;
                weaponHolder.localRotation = Quaternion.Euler(weaponHolderRotation);
            }

            // Add CharacterController if not present
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.height = 2f;
                characterController.radius = 0.3f;
                characterController.center = new Vector3(0, -cameraHeight + 1f, 0); // Center below camera
            }

            // Lock cursor
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Debug.Log("[DesktopTester] Desktop Weapon Tester initialized!");
            Debug.Log("[DesktopTester] VR HEADSET MODE - Camera controlled by headset only");
            Debug.Log("[DesktopTester] Controls: Left Click - Shoot, E - Pickup Weapon, R - Reload, ESC - Unlock Cursor");
        }

        private void Update()
        {
            // CAMERA MOVEMENT DISABLED - Use VR headset only
            // HandleMovement();
            // HandleLook();
            HandleWeaponPickup();
            HandleShooting();
            HandleCursorToggle();
        }

        private void HandleMovement()
        {
            // Get input using new Input System or legacy
            float horizontal = 0f;
            float vertical = 0f;

            // Try new Input System first
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) vertical = 1f;
                if (Keyboard.current.sKey.isPressed) vertical = -1f;
                if (Keyboard.current.aKey.isPressed) horizontal = -1f;
                if (Keyboard.current.dKey.isPressed) horizontal = 1f;
            }
            else
            {
                // Fallback to legacy input
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
            }

            // Calculate movement direction
            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

            // Apply gravity
            if (!characterController.isGrounded)
            {
                moveDirection.y = -9.81f;
            }

            // Move
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        private void HandleLook()
        {
            float mouseX = 0f;
            float mouseY = 0f;

            // Get mouse input
            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                mouseX = mouseDelta.x;
                mouseY = mouseDelta.y;
            }
            else
            {
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");
            }

            // Rotate camera
            rotationX -= mouseY * lookSensitivity;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y + mouseX * lookSensitivity, 0f);
        }

        private void HandleWeaponPickup()
        {
            bool pickupPressed = false;

            // Check for E key press
            if (Keyboard.current != null)
            {
                pickupPressed = Keyboard.current.eKey.wasPressedThisFrame;
            }
            else
            {
                pickupPressed = Input.GetKeyDown(KeyCode.E);
            }

            if (pickupPressed)
            {
                if (currentWeapon != null)
                {
                    // Drop weapon
                    DropWeapon();
                }
                else
                {
                    // Try to pickup weapon
                    TryPickupWeapon();
                }
            }
        }

        private void TryPickupWeapon()
        {
            // Raycast to find weapon
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange))
            {
                VRWeaponController weapon = hit.collider.GetComponent<VRWeaponController>();

                if (weapon != null)
                {
                    PickupWeapon(weapon);
                }
            }
            else
            {
                // Try to find any weapon in range
                Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);
                foreach (Collider col in colliders)
                {
                    VRWeaponController weapon = col.GetComponent<VRWeaponController>();
                    if (weapon != null)
                    {
                        PickupWeapon(weapon);
                        break;
                    }
                }
            }
        }

        private void PickupWeapon(VRWeaponController weapon)
        {
            currentWeapon = weapon;

            // Parent weapon to weapon holder
            weapon.transform.SetParent(weaponHolder);
            weapon.transform.localPosition = weaponLocalPosition;
            weapon.transform.localRotation = Quaternion.Euler(weaponLocalRotation);
            weapon.transform.localScale = Vector3.one * weaponScale;

            // Disable rigidbody if present
            Rigidbody rb = weapon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // IMPORTANT: Enable desktop mode on weapon
            // Use reflection to set isGrabbed to true since it's private
            var isGrabbedField = typeof(VRWeaponController).GetField("isGrabbed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (isGrabbedField != null)
            {
                isGrabbedField.SetValue(weapon, true);
                Debug.Log($"[DesktopTester] Set weapon as grabbed (desktop mode)");
            }

            // Enable laser sight if available
            weapon.SetLaserSightEnabled(true);

            Debug.Log($"[DesktopTester] Picked up weapon: {weapon.name}");
            Debug.Log($"[DesktopTester] Weapon holder position (world): {weaponHolder.position}");
            Debug.Log($"[DesktopTester] Weapon position (world): {weapon.transform.position}");
            Debug.Log($"[DesktopTester] Weapon local position: {weapon.transform.localPosition}");
            Debug.Log($"[DesktopTester] Weapon local rotation: {weapon.transform.localEulerAngles}");
        }

        private void DropWeapon()
        {
            if (currentWeapon == null) return;

            // Set isGrabbed to false
            var isGrabbedField = typeof(VRWeaponController).GetField("isGrabbed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (isGrabbedField != null)
            {
                isGrabbedField.SetValue(currentWeapon, false);
            }

            // Disable laser sight
            currentWeapon.SetLaserSightEnabled(false);

            // Unparent weapon
            currentWeapon.transform.SetParent(null);

            // Enable rigidbody
            Rigidbody rb = currentWeapon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            Debug.Log($"[DesktopTester] Dropped weapon: {currentWeapon.name}");
            currentWeapon = null;
        }

        private void HandleShooting()
        {
            if (currentWeapon == null) return;

            bool shootPressed = false;

            // Check for left mouse button
            if (Mouse.current != null)
            {
                shootPressed = Mouse.current.leftButton.isPressed;
            }
            else
            {
                shootPressed = Input.GetMouseButton(0);
            }

            if (shootPressed)
            {
                currentWeapon.TryShoot();
            }

            // Reload with R key
            bool reloadPressed = false;
            if (Keyboard.current != null)
            {
                reloadPressed = Keyboard.current.rKey.wasPressedThisFrame;
            }
            else
            {
                reloadPressed = Input.GetKeyDown(KeyCode.R);
            }

            if (reloadPressed)
            {
                currentWeapon.StartReload();
            }
        }

        private void HandleCursorToggle()
        {
            bool escapePressed = false;

            if (Keyboard.current != null)
            {
                escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
            }
            else
            {
                escapePressed = Input.GetKeyDown(KeyCode.Escape);
            }

            if (escapePressed)
            {
                cursorLocked = !cursorLocked;
                Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !cursorLocked;
            }
        }

        private void OnDrawGizmos()
        {
            if (!showPickupRange) return;

            // Draw pickup range sphere
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, pickupRange);

            // Check if weapon is in range
            VRWeaponController nearbyWeapon = FindNearbyWeapon();
            if (nearbyWeapon != null)
            {
                // Draw line to nearby weapon
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, nearbyWeapon.transform.position);

                // Draw sphere at weapon
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(nearbyWeapon.transform.position, 0.2f);
            }
        }

        private VRWeaponController FindNearbyWeapon()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);
            foreach (Collider col in colliders)
            {
                VRWeaponController weapon = col.GetComponent<VRWeaponController>();
                if (weapon != null && weapon != currentWeapon)
                {
                    return weapon;
                }
            }
            return null;
        }

        private void OnGUI()
        {
            // Draw simple UI
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, 10, 400, 30), "VR HEADSET MODE - Camera by Headset", style);
            GUI.Label(new Rect(10, 40, 400, 25), "E - Pickup/Drop | Left Click - Shoot | R - Reload", style);
            GUI.Label(new Rect(10, 65, 400, 25), "ESC - Cursor | Camera controlled by VR headset", style);

            if (currentWeapon != null)
            {
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(10, 100, 400, 25), $"Weapon: {currentWeapon.name}", style);
                GUI.Label(new Rect(10, 125, 400, 25), $"Ammo: {currentWeapon.CurrentAmmo}/{currentWeapon.MaxAmmo}", style);

                if (currentWeapon.IsReloading)
                {
                    style.normal.textColor = Color.red;
                    GUI.Label(new Rect(10, 150, 400, 25), "RELOADING...", style);
                }
            }
            else
            {
                // Check if weapon is nearby
                VRWeaponController nearbyWeapon = FindNearbyWeapon();
                if (nearbyWeapon != null)
                {
                    style.normal.textColor = Color.green;
                    GUI.Label(new Rect(10, 100, 400, 25), $"Press E to pickup: {nearbyWeapon.name}", style);

                    float distance = Vector3.Distance(transform.position, nearbyWeapon.transform.position);
                    GUI.Label(new Rect(10, 125, 400, 25), $"Distance: {distance:F1}m", style);
                }
                else
                {
                    style.normal.textColor = Color.gray;
                    GUI.Label(new Rect(10, 100, 400, 25), "No weapon equipped - Press E near a weapon", style);
                }
            }
        }
    }
}
