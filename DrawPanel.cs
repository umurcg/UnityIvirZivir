using System;
using Dreamteck.Splines;
using Lean.Touch;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a spline on the screen in range of the image assigned to this gameobject
/// </summary>

[RequireComponent(typeof(Image))]
public class DrawPanel : MonoBehaviour
{
    private Image _image;
    private SplineComputer _activeSpline;
    private PathGenerator _pathGenerator;
    private Camera _mainCamera;
    [ReadOnly]private float _tolerance = 0.1f;
    private Vector2 lastSplinedPoint;
    [SerializeField] private float drawDistance = 10f;
    [SerializeField] private float width = 0.1f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _image = GetComponent<Image>();
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUpdate += OnFingerUpdate;
        LeanTouch.OnFingerUp += OnFingerUp;
    }

    private void OnFingerDown(LeanFinger obj)
    {
        _activeSpline = new GameObject("Spline").AddComponent<SplineComputer>();
        _activeSpline.type = Spline.Type.Linear;

        _pathGenerator= _activeSpline.gameObject.AddComponent<PathGenerator>();
        _pathGenerator.size = width;
        
        var meshRenderer = _activeSpline.gameObject.GetOrAddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/Color"));
        meshRenderer.material.color = Color.red;
        _pathGenerator.spline = _activeSpline;
    }

    private void OnFingerUp(LeanFinger finger)
    {
        BreakSpline();
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (_activeSpline == null) return;

        if (!IsPositionOnImage(finger.ScreenPosition))
        {
            BreakSpline();
        }
        
        var delta= finger.ScreenPosition - lastSplinedPoint;
        if (delta.magnitude > _tolerance)
        {
            AddPointToSpline(finger.ScreenPosition);
        }
    }

    private void BreakSpline()
    {
        if (_activeSpline == null) return;
        Destroy(_activeSpline.gameObject);
        _activeSpline = null;
        _pathGenerator = null;
    }

    private bool IsPositionOnImage(Vector2 pos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_image.rectTransform, pos, null);
    }
    
    private void AddPointToSpline(Vector2 screenPoint)
    {
        if (_activeSpline == null)
        {
            return;
        }
        
        var worldPoint = _mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, drawDistance));
        var splinePoint = new SplinePoint(worldPoint);
        splinePoint.normal= -_mainCamera.transform.forward;
        _activeSpline.SetPoint(_activeSpline.pointCount, splinePoint);
        _activeSpline.Rebuild();
        lastSplinedPoint = screenPoint;

    }

    private void OnDestroy()
    {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }
}

