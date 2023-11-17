
using System;

namespace Illumine.LPR
{
    public interface ICameraService
    {
        void Init();

        void SetCallback(CameraViewerViewModel cameraViewerViewModel, Action<LPRCameraArgs> Callback);
        void SetBackCallback(CameraViewerViewModel cameraViewerViewModel, Action<LPRCameraArgs> Callback);

        void StartVideo(IntPtr camId, IntPtr handle);

        void StopVideo(IntPtr camId);

        void CheckValid(ref CameraViewerViewModel cvvm);

        bool OpenDoor(IntPtr camId);

        bool Connect(ref CameraViewerViewModel cameraViewerViewModel);

        void Disconnect(ref CameraViewerViewModel cameraViewerViewModel);

        bool BackConnect(ref CameraViewerViewModel cameraViewerViewModel);

        void BackTrigger(IntPtr camId, int index);

        void Trigger(IntPtr camId, int index);

        void Send485(IntPtr camId, byte[] data);
    }
}
