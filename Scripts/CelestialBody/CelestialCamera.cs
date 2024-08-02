using Godot;
using RoemersLegacy.Scripts;
using System;

public partial class CelestialCamera : Node3D
{
	private float _cameraPanSpeed = 1000.0f;
	public float CameraPanSpeed
	{
		get => _cameraPanSpeed;
		set
		{
			_cameraPanSpeed = Mathf.Max(1, value);
		}
	}

	private float _cameraRotationSpeed = 1.0f;
	public float CameraRotationSpeed
	{
		get => _cameraRotationSpeed;
		set
		{
			_cameraRotationSpeed = Mathf.Max(1, value);
		}
	}

	private int _cameraSpeedScale = 1;
	public int CameraSpeedScale
	{
		get => _cameraSpeedScale;
		private set
		{
			_cameraSpeedScale = Mathf.Max(1, value);
		}
	}

	public void IncreaseCameraSpeed()
	{
		CameraSpeedScale += (int)Math.Pow(10, Math.Floor(Math.Log10(CameraSpeedScale)));
		GD.Print($"Camera speed scale: {CameraSpeedScale}");
	}

	public void DecreaseCameraSpeed()
	{
		CameraSpeedScale -= CameraSpeedScale != 1 ? (int)Math.Pow(10, Math.Floor(Math.Log10(CameraSpeedScale - 1))) : 0;
		GD.Print($"Camera speed scale: {CameraSpeedScale}");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// retrieve inputs for camera_pan_up/down/left/right, camera_pitch_up/down, 
		// camera_zoom_in/out, camera_roll_cw/ccw, and camera_yaw_left/right
		UpdateCameraPosition(delta);
	}

	private void UpdateCameraPosition(double delta)
	{
		var cameraMovement = new Vector3();
		var cameraRotation = new Vector3();

		if (InputHelper.IsActionPressedOrHeld("camera_pan_up"))
		{
			cameraMovement.Y -= 1;
		}
		else if (InputHelper.IsActionPressedOrHeld("camera_pan_down"))
		{
			cameraMovement.Y += 1;
		}

		if (InputHelper.IsActionPressedOrHeld("camera_pan_left"))
		{
			cameraMovement.X -= 1;
		}
		else if (InputHelper.IsActionPressedOrHeld("camera_pan_right"))
		{
			cameraMovement.X += 1;
		}

		if (InputHelper.IsActionPressedOrHeld("camera_zoom_in") || InputHelper.IsActionPressedOrHeld("camera_pan_in"))
		{
			cameraMovement.Z -= 1;
		}
		else if (InputHelper.IsActionPressedOrHeld("camera_zoom_out") || InputHelper.IsActionPressedOrHeld("camera_pan_out"))
		{
			cameraMovement.Z += 1;
		}

		if (InputHelper.IsActionPressedOrHeld("camera_pitch_up"))
		{
			cameraRotation.X += 1;
		}
		else if (InputHelper.IsActionPressedOrHeld("camera_pitch_down"))
		{
			cameraRotation.X -= 1;
		}

		if (InputHelper.IsActionPressedOrHeld("camera_rotate_cw"))
		{
			cameraRotation.Z -= 1;
		}
		else if (InputHelper.IsActionPressedOrHeld("camera_rotate_ccw"))
		{
			cameraRotation.Z += 1;
		}

		if (InputHelper.IsActionPressedOrHeld("camera_yaw_left"))
		{
			cameraRotation.Y -= 1;
		}
		else if (InputHelper.IsActionPressedOrHeld("camera_yaw_right"))
		{
			cameraRotation.Y += 1;
		}

		if (Input.IsActionJustReleased("camera_speed_scale_increase"))
		{
			IncreaseCameraSpeed();
		}
		else if (Input.IsActionJustReleased("camera_speed_scale_decrease"))
		{
			DecreaseCameraSpeed();
		}

		if (Input.IsActionJustPressed("camera_pan_speed_increase"))
		{
			CameraPanSpeed += CameraSpeedScale;
			GD.Print($"Camera pan speed: {CameraPanSpeed}");
		}
		else if (Input.IsActionJustPressed("camera_pan_speed_decrease"))
		{
			CameraPanSpeed -= CameraSpeedScale;
			GD.Print($"Camera pan speed: {CameraPanSpeed}");
		}

		if (Input.IsActionJustPressed("camera_rotate_speed_increase"))
		{
			CameraRotationSpeed += CameraSpeedScale;
			GD.Print($"Camera rotation speed: {CameraRotationSpeed}");
		}
		else if (Input.IsActionJustPressed("camera_rotate_speed_decrease"))
		{
			CameraRotationSpeed -= CameraSpeedScale;
			GD.Print($"Camera rotation speed: {CameraRotationSpeed}");
		}

		// Transform movement according to camera's local orientation
		cameraMovement = cameraMovement.Normalized();
		Vector3 transformedMovement = GlobalTransform.Basis * cameraMovement;
		transformedMovement *= CameraPanSpeed * (float)delta;
		Position += transformedMovement;

		// Handle mouse rotation and capture
		if (Input.IsMouseButtonPressed(MouseButton.Middle))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;

			Vector2 mouseMovement = Input.GetLastMouseVelocity();
			cameraRotation.Y -= mouseMovement.X * CameraRotationSpeed * (float)delta;
			cameraRotation.X -= mouseMovement.Y * CameraRotationSpeed * (float)delta;
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Visible; // Release mouse when middle button is not pressed
		}

		// Apply rotation based on local orientation
		RotateCamera(cameraRotation, delta);
	}

	private void RotateCamera(Vector3 cameraRotation, double delta)
	{
		if (cameraRotation == Vector3.Zero)
		{
			return;
		}

		// Create quaternions for pitch (X) and yaw (Y) rotations
		Quaternion pitch = new Quaternion(Vector3.Right, cameraRotation.X * (float)delta);
		Quaternion yaw = new Quaternion(Vector3.Up, cameraRotation.Y * (float)delta);
		Quaternion roll = new Quaternion(Vector3.Forward, cameraRotation.Z * (float)delta);

		// Combine the rotations
		Quaternion combinedRotation = (yaw * pitch * roll).Normalized();

		var currentRotation = Transform.Basis.GetRotationQuaternion();

		// Apply the combined rotation to the camera
		Basis = new Basis(currentRotation * combinedRotation);
	}
}
