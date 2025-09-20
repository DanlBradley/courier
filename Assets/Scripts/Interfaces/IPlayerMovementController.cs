using UnityEngine;

namespace Interfaces
{
    public interface IPlayerMovementController
    {
        bool IsMoving { get; }
        bool IsSprinting { get; }
        bool IsCrouching { get; }
        bool IsGrounded { get; }
        float CurrentSpeed { get; }
        bool IsWalking();
        bool IsRunning();
        bool IsClimbing();
        bool IsFalling();
        Vector3 GetMovementDirection();
        float GetVerticalVelocity();
        Vector3 GetVelocity();
        void Teleport(Vector3 position);
        void ResetMovement();
    }
}